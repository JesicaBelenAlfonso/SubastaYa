using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Bids.Commands;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Bids.Handlers
{
    public class CreateBidCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IBidRepository _bids;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;

        public CreateBidCommandHandler(
            IAuctionRepository auctions,
            IBidRepository bids,
            IUnitOfWork uow,
            IAuditService audit)
        {
            _auctions = auctions;
            _bids = bids;
            _uow = uow;
            _audit = audit;
        }

        public async Task<BidResponseDto> Handle(CreateBidCommand cmd)
        {
            var auction = await _auctions.GetByIdAsync(cmd.AuctionId)
                ?? throw new DomainException($"No existe una subasta con Id {cmd.AuctionId}");

            var currentOffer = await _bids.GetHighestAmountByAuctionIdAsync(cmd.AuctionId);
            var minimum = currentOffer ?? auction.BasePrice;

            if (cmd.Amount <= minimum)
            {
                var reference = currentOffer.HasValue ? "la oferta actual" : "el precio base";
                throw new DomainException($"La puja debe ser mayor a {reference} ({minimum})");
            }

            var bid = cmd.ToEntity(cmd.BuyerId);

            await _bids.AddAsync(bid);
            await _uow.SaveChangesAsync();

            await _audit.LogAsync("Bid", bid.Id, "CREATE", cmd.BuyerId, new
            {
                bid.AuctionId,
                bid.Amount
            });

            return bid.ToDto();
        }
    }
}
