using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Bids.Commands;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Bids.Handlers
{
    public class CreateBidCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IBidRepository _bids;
        private readonly IWalletRepository _wallets;     
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;
        private readonly AuctionOptions _options;

        public CreateBidCommandHandler(
            IAuctionRepository auctions,
            IBidRepository bids,
            IWalletRepository wallets,      
            IUnitOfWork uow,
            IAuditService audit,
            AuctionOptions options)
        {
            _auctions = auctions;
            _bids = bids;
            _wallets = wallets;            
            _uow = uow;
            _audit = audit;
            _options = options;
        }

        public async Task<BidResponseDto> Handle(CreateBidCommand cmd)
        {
            var auction = await _auctions.GetByIdAsync(cmd.AuctionId) //Valida su existe subasta
                ?? throw new DomainException($"No existe una subasta con Id {cmd.AuctionId}");

            if (auction.Status != "ACTIVA")
            {
                await RejectAsync(cmd, auction, "La subasta no está activa");
                throw new DomainException("La subasta no está activa");
            }

            //Calculo del monto min requerido
            var currentOffer = await _bids.GetHighestAmountByAuctionIdAsync(cmd.AuctionId);
            var minimum = currentOffer ?? auction.BasePrice;

            // La puja debe ser al menos igual a (oferta actual + incremento mínimo oficial)
            var requiredAmount = minimum + auction.MinIncrement;
            if (cmd.Amount < requiredAmount)
            {
                var reference = currentOffer.HasValue ? "la oferta actual" : "el precio base";
                await RejectAsync(cmd, auction, $"La puja debe ser mayor o igual a {reference} ({minimum}) + incremento mínimo ({auction.MinIncrement}) = {requiredAmount}");
                throw new DomainException($"La puja debe ser mayor o igual a {reference} ({minimum}) + incremento mínimo ({auction.MinIncrement}) = {requiredAmount}");
            }

            await ApplyAntiSniping(auction);

            // Verificamos que el usuario tenga una wallet asociada
            var wallet = await _wallets.GetByUserIdAsync(cmd.BuyerId)
                ?? throw new DomainException($"El usuario id {cmd.BuyerId} no tiene una billetera asociada");

            // Validamos que el usuario tenga suficiente saldo disponible
            if (wallet.AvailableBalance < cmd.Amount)
            {
                throw new DomainException($"Saldo disponible insuficiente para tu puja. Disponible: {wallet.AvailableBalance}, Requerido: {cmd.Amount}");
            }

            // Bloqueamos el monto para que el usuario no pueda usarlo mientras su puja esté activa
            wallet.HeldBalance += cmd.Amount;

            var bid = cmd.ToEntity(cmd.BuyerId); // Creamos la entidad Bid 

            // Después de crear la puja, actualizamos el precio base de la subasta
            // con el nuevo monto de la puja ganadora
            auction.BasePrice = cmd.Amount;

            await _bids.AddAsync(bid);

            await _audit.LogAsync("Bid", bid.Id, AuditActions.CREATE, cmd.BuyerId, new //Registro de Auditoría
            {
                bid.AuctionId,
                bid.Amount
            });

            try
            {
                await _uow.SaveChangesAsync(); // Guardamos: Bid + Wallet + Auction de forma atómica
            }
            catch (DbUpdateConcurrencyException)
            {
                await RejectAsync(cmd, auction, "Conflicto de concurrencia al registrar la puja");
                throw;
            }

            return bid.ToDto();
        }

        private async Task ApplyAntiSniping(Auction auction)
        {
            var remaining = auction.EndDate - DateTime.UtcNow;
            if (remaining <= TimeSpan.FromMinutes(_options.AntiSnipingWindowMinutes))
            {
                var previousEnd = auction.EndDate;
                auction.EndDate = previousEnd.AddMinutes(_options.AntiSnipingExtensionMinutes);

                await _audit.LogAsync("Auction", auction.Id, AuditActions.AUCTION_TIME_EXTENDED, 0, new
                {
                    auctionId = auction.Id,
                    previousEnd,
                    newEnd = auction.EndDate
                });
            }
        }

        private async Task RejectAsync(CreateBidCommand cmd, Auction auction, string reason)
        {
            await _audit.LogAsync("Bid", cmd.AuctionId, AuditActions.BID_REJECTED, cmd.BuyerId, new
            {
                auctionId = cmd.AuctionId,
                amount = cmd.Amount,
                minimum = await _bids.GetHighestAmountByAuctionIdAsync(cmd.AuctionId) ?? auction.BasePrice,
                auctionStatus = auction.Status,
                reason
            });

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch
            {
                // La auditoría es best-effort: no debe enmascarar el error original.
            }
        }
    }
}