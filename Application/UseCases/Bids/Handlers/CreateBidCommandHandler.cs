using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Bids.Commands;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Bids.Handlers
{
    public class CreateBidCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IBidRepository _bids;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;
        private readonly AuctionOptions _options;

        public CreateBidCommandHandler(
            IAuctionRepository auctions,
            IBidRepository bids,
            IUnitOfWork uow,
            IAuditService audit,
            AuctionOptions options)
        {
            _auctions = auctions;
            _bids = bids;
            _uow = uow;
            _audit = audit;
            _options = options;
        }

        public async Task<BidResponseDto> Handle(CreateBidCommand cmd)
        {
            var auction = await _auctions.GetByIdAsync(cmd.AuctionId)
                ?? throw new DomainException($"No existe una subasta con Id {cmd.AuctionId}");

            if (auction.Status != "ACTIVA")
            {
                await RejectAsync(cmd, auction, "La subasta no está activa");
                throw new DomainException("La subasta no está activa");
            }

            var currentOffer = await _bids.GetHighestAmountByAuctionIdAsync(cmd.AuctionId);
            var minimum = currentOffer ?? auction.BasePrice;

            if (cmd.Amount <= minimum)
            {
                var reference = currentOffer.HasValue ? "la oferta actual" : "el precio base";
                await RejectAsync(cmd, auction, $"La puja debe ser mayor a {reference} ({minimum})");
                throw new DomainException($"La puja debe ser mayor a {reference} ({minimum})");
            }

            await ApplyAntiSniping(auction);

            var bid = cmd.ToEntity(cmd.BuyerId);

            await _bids.AddAsync(bid);

            await _audit.LogAsync("Bid", bid.Id, AuditActions.CREATE, cmd.BuyerId, new
            {
                bid.AuctionId,
                bid.Amount
            });

            try
            {
                await _uow.SaveChangesAsync();
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