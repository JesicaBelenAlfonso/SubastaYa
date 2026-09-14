using Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface IBidRepository
    {
        Task AddAsync(Bid bid);
        Task<decimal?> GetHighestAmountByAuctionIdAsync(int auctionId);
    }
}
