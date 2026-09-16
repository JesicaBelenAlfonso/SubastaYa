using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface IBidRepository
    {
        Task AddAsync(Bid bid);
        Task<decimal?> GetHighestAmountByAuctionIdAsync(int auctionId);
        Task<Bid?> GetHighestBidByAuctionIdAsync(int auctionId);
        Task<int> GetCountByAuctionIdAsync(int auctionId);
        Task<IEnumerable<Bid>> GetByAuctionIdAsync(int auctionId);
        Task<bool> HasBidAsync(int auctionId, int buyerId);
    }
}
