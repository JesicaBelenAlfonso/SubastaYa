using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Bids.Commands;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;
using System;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Bids.Handlers
{
    public class CreateBidCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IBidRepository _bids;
        private readonly IWalletRepository _wallets;
        private readonly ITransactionRepository _transactions;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;
        private readonly AuctionOptions _options;

        public CreateBidCommandHandler(
            IAuctionRepository auctions,
            IBidRepository bids,
            IWalletRepository wallets,
            ITransactionRepository transactions,
            IUnitOfWork uow,
            IAuditService audit,
            AuctionOptions options)
        {
            _auctions = auctions;
            _bids = bids;
            _wallets = wallets;
            _transactions = transactions;
            _uow = uow;
            _audit = audit;
            _options = options;
        }

        public async Task<BidResponseDto> Handle(CreateBidCommand cmd)
        {
            var now = DateTime.UtcNow;

            var auction = await _auctions.GetByIdAsync(cmd.AuctionId);
            if (auction is null)
            {
                // Se audita igual el intento, aunque el recurso no exista.
                await RejectAsync(cmd, null, cmd.AuctionId, "No existe la subasta");
                throw new NotFoundException($"No existe una subasta con Id {cmd.AuctionId}");
            }

            // Activación automática: si ya empezó, PROXIMA pasa a ACTIVA.
            auction.RefreshStatus(now);

            if (auction.Status != AuctionStatus.Activa)
            {
                await RejectAsync(cmd, auction, auction.Id, "La subasta no está activa");
                throw new DomainConflictException("La subasta no está activa");
            }

            // El vendedor no puede pujar su propia subasta (evita pujas fantasma).
            if (cmd.BuyerId == auction.SellerId)
            {
                await RejectAsync(cmd, auction, auction.Id, "El vendedor no puede pujar su propia subasta");
                throw new DomainConflictException("No podés pujar tu propia subasta");
            }

            // Si el tiempo ya se agotó, se rechaza (el worker la finalizará).
            var remaining = auction.EndDate - now;
            if (remaining <= TimeSpan.Zero)
            {
                await RejectAsync(cmd, auction, auction.Id, "La subasta ya venció");
                throw new DomainConflictException("La subasta ya venció");
            }

            // Monto mínimo requerido.
            var previousLeader = await _bids.GetHighestBidByAuctionIdAsync(cmd.AuctionId);
            var currentOffer = previousLeader?.Amount;
            var minimum = currentOffer ?? auction.BasePrice;

            // La puja debe ser al menos igual a (oferta actual + incremento mínimo oficial)
            var requiredAmount = minimum + auction.MinIncrement;
            if (cmd.Amount < requiredAmount)
            {
                var reference = currentOffer.HasValue ? "la oferta actual" : "el precio base";
                await RejectAsync(cmd, auction, auction.Id, $"La puja debe ser mayor o igual a {reference} ({minimum}) + incremento mínimo ({auction.MinIncrement}) = {requiredAmount}");
                throw new DomainException($"La puja debe ser mayor o igual a {reference} ({minimum}) + incremento mínimo ({auction.MinIncrement}) = {requiredAmount}");
            }

            // Wallet del pujador.
            var wallet = await _wallets.GetByUserIdAsync(cmd.BuyerId);
            if (wallet is null)
            {
                await RejectAsync(cmd, auction, auction.Id, "El usuario no tiene una billetera asociada");
                throw new DomainException($"El usuario id {cmd.BuyerId} no tiene una billetera asociada");
            }

            // Si ya era el líder, solo cubre la diferencia (delta).
            var alreadyHeld = previousLeader is not null && previousLeader.BuyerId == cmd.BuyerId
                ? previousLeader.Amount
                : 0m;

            if (wallet.AvailableBalance + alreadyHeld < cmd.Amount)
            {
                await RejectAsync(cmd, auction, auction.Id, "Saldo disponible insuficiente");
                throw new DomainConflictException($"Saldo disponible insuficiente para tu puja. Disponible: {wallet.AvailableBalance + alreadyHeld}, Requerido: {cmd.Amount}");
            }

            // --- Escrow ---

            // 1) Liberamos la retención del líder anterior.
            if (previousLeader is not null)
            {
                var previousWallet = previousLeader.BuyerId == cmd.BuyerId
                    ? wallet
                    : await _wallets.GetByUserIdAsync(previousLeader.BuyerId);

                if (previousWallet is not null)
                {
                    previousWallet.HeldBalance -= previousLeader.Amount;

                    await _transactions.AddAsync(new Transaction
                    {
                        WalletId = previousWallet.Id,
                        Type = TransactionType.Liberacion,
                        Amount = previousLeader.Amount,
                        AuctionId = auction.Id,
                        Date = now
                    });
                }
            }

            // 2) Retenemos el monto de la nueva puja ganadora.
            wallet.HeldBalance += cmd.Amount;

            await _transactions.AddAsync(new Transaction
            {
                WalletId = wallet.Id,
                Type = TransactionType.Retencion,
                Amount = cmd.Amount,
                AuctionId = auction.Id,
                Date = now
            });

            // 3) Anti-sniping: extiende el fin si la puja entra en la ventana crítica.
            await ApplyAntiSniping(auction, cmd.BuyerId, remaining);

            // Toca la subasta en cada puja (marca de actividad + RowVersion).
            auction.LastBidAt = now;

            var bid = cmd.ToEntity(cmd.BuyerId);
            await _bids.AddAsync(bid);

            await _audit.LogAsync("Bid", auction.Id, AuditAction.CREATE, cmd.BuyerId, new
            {
                bid.AuctionId,
                bid.Amount
            });

            // --- TRANSACCIONALIDAD ATÓMICA (ACID) ---
            await _uow.BeginTransactionAsync();

            try
            {
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackAsync();

                // Se desanexa todo para guardar solo la auditoría de rechazo.
                _uow.DetachAll();

                await RejectAsync(cmd, auction, auction.Id, "Conflicto de concurrencia al registrar la puja");
                throw;
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }

            return bid.ToDto();
        }

        private async Task ApplyAntiSniping(Auction auction, int bidderId, TimeSpan remaining)
        {
            var window = TimeSpan.FromSeconds(_options.AntiSnipingWindowSeconds);
            if (remaining > TimeSpan.Zero && remaining <= window)
            {
                var previousEnd = auction.EndDate;
                auction.EndDate = previousEnd.AddMinutes(_options.AntiSnipingExtensionMinutes);

                await _audit.LogAsync("Auction", auction.Id, AuditAction.AUCTION_TIME_EXTENDED, bidderId, new
                {
                    auctionId = auction.Id,
                    previousEnd,
                    newEnd = auction.EndDate,
                    extendedByMinutes = _options.AntiSnipingExtensionMinutes
                });
            }
        }

        private async Task RejectAsync(CreateBidCommand cmd, Auction? auction, int auctionId, string reason)
        {
            try
            {
                // Best-effort: persistir la auditoría sin reintentar operaciones mutantes.
                await _audit.LogAsync("Bid", auctionId, AuditAction.BID_REJECTED, cmd.BuyerId, new
                {
                    auctionId,
                    amount = cmd.Amount,
                    minimum = await _bids.GetHighestAmountByAuctionIdAsync(auctionId) ?? auction?.BasePrice,
                    auctionStatus = auction?.Status.ToString().ToUpperInvariant(),
                    reason
                });

                await _uow.SaveChangesAsync();
            }
            catch
            {
                // La auditoría es best-effort: no debe enmascarar el error original.
            }
        }
    }
}