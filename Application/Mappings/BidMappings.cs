using SubastaYa.Domain.Entities;
using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.Mappings
{
    public static class BidMappings
    {
        public static Bid ToEntity(this CreateBidDto dto, int buyerId)
        {
            return new Bid
            {
                BuyerId = buyerId,
                AuctionId = dto.AuctionId,
                Amount = dto.Amount,
                BidDate = DateTime.UtcNow
            };
        }

        public static BidResponseDto ToDto(this Bid bid)
        {
            return new BidResponseDto
            {
                Id = bid.Id,
                BuyerId = bid.BuyerId,
                AuctionId = bid.AuctionId,
                Amount = bid.Amount,
                BidDate = bid.BidDate
            };
        }
    }
}
