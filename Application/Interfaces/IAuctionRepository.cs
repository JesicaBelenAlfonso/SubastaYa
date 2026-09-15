using SubastaYa.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubastaYa.Application.Interfaces
{
    public interface IAuctionRepository
    {
        Task AddAsync(Auction auction);
        Task<IEnumerable<Auction>> GetAllAsync();
        Task<Auction?> GetByIdAsync(int id);
        Task<IEnumerable<Auction>> GetAllAsync(string? status, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy);
        Task DeleteAsync(Auction auction);
        void Delete(Auction auction);
    }
}