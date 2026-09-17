using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface IAuctionRepository
    {
        Task AddAsync(Auction auction);
        Task<IEnumerable<Auction>> GetAllAsync();
        Task<Auction?> GetByIdAsync(int id);
        void Delete(Auction auction);
        Task<IEnumerable<Auction>> GetExpiredActiveAsync(DateTime now);
    }
}
