using Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface IAuctionRepository
    {
        Task AddAsync(Auction auction);
        Task<Auction?> GetByIdAsync(int id);
        void Delete(Auction auction);
    }
}
