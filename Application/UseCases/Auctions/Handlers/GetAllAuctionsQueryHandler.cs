using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Auctions.Queries;
using System.Linq;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class GetAllAuctionsQueryHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly ICategoryRepository _categories;
        private readonly IBidRepository _bids;

        public GetAllAuctionsQueryHandler(
            IAuctionRepository auctions,
            ICategoryRepository categories,
            IBidRepository bids)
        {
            _auctions = auctions;
            _categories = categories;
            _bids = bids;
        }

        public async Task<List<AuctionResponseDto>> Handle(GetAllAuctionsQuery query)
        {
            var auctions = await _auctions.GetAllAsync();
            var categories = await _categories.GetAllAsync();
            var categorias = categories.ToDictionary(c => c.Id, c => c.Name);

            var result = new List<AuctionResponseDto>();
            foreach (var auction in auctions)
            {
                var categoria = categorias.TryGetValue(auction.CategoryId, out var name)
                    ? name
                    : null;

                result.Add(auction.ToDto(
                    categoria,
                    await _bids.GetHighestAmountByAuctionIdAsync(auction.Id),
                    await _bids.GetCountByAuctionIdAsync(auction.Id)));
            }

            return result;
        }
    }
}