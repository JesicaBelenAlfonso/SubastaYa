using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Auctions.Queries;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class GetAuctionByIdQueryHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly ICategoryRepository _categories;
        private readonly IBidRepository _bids;

        public GetAuctionByIdQueryHandler(
            IAuctionRepository auctions,
            ICategoryRepository categories,
            IBidRepository bids)
        {
            _auctions = auctions;
            _categories = categories;
            _bids = bids;
        }

        public async Task<AuctionResponseDto?> Handle(GetAuctionByIdQuery query)
        {
            var auction = await _auctions.GetByIdAsync(query.Id);

            if (auction is null)
                return null;

            auction.RefreshStatus(DateTime.UtcNow);

            var categoria = await _categories.GetByIdAsync(auction.CategoryId);

            return auction.ToDto(
                categoria?.Name,
                await _bids.GetHighestAmountByAuctionIdAsync(auction.Id),
                await _bids.GetCountByAuctionIdAsync(auction.Id));
        }
    }
}