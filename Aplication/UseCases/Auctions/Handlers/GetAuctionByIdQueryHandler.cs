using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Auctions.Queries;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class GetAuctionByIdQueryHandler
    {
        private readonly IAuctionRepository _auctions;

        public GetAuctionByIdQueryHandler(IAuctionRepository auctions)
        {
            _auctions = auctions;
        }

        public async Task<AuctionResponseDto?> Handle(GetAuctionByIdQuery query)
        {
            var auction = await _auctions.GetByIdAsync(query.Id);

            if (auction is null)
                return null;

            return auction.ToDto();
        }
    }
}
