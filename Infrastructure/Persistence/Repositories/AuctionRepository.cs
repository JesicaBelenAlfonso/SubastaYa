using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces;
using System.Threading.Tasks;

namespace SubastaYa.Infrastructure.Persistence.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AppDbContext _ctx;

        public AuctionRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(Auction auction)
        {
            await _ctx.Auctions.AddAsync(auction);
        }

        public async Task<IEnumerable<Auction>> GetAllAsync()
            => await _ctx.Auctions.OrderByDescending(a => a.StartDate).ToListAsync();

        public async Task<Auction?> GetByIdAsync(int id)
            => await _ctx.Auctions.FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Auction>> GetAllAsync(string? status, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var query = _ctx.Auctions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status == status);

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(a => a.BasePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(a => a.BasePrice <= maxPrice.Value);

            query = sortBy?.ToLowerInvariant() switch
            {
                "price" => query.OrderBy(a => a.BasePrice),
                "price_desc" => query.OrderByDescending(a => a.BasePrice),
                "enddate" => query.OrderBy(a => a.EndDate),
                "title" => query.OrderBy(a => a.Title),
                _ => query.OrderByDescending(a => a.StartDate)
            };

            return await query.ToListAsync();
        }

        public void Delete(Auction auction)
            => _ctx.Auctions.Remove(auction);

        public Task DeleteAsync(Auction auction)
        {
            _ctx.Auctions.Remove(auction);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Auction>> GetExpiredActiveAsync(DateTime now)
            => await _ctx.Auctions
                .Where(a => a.Status == "ACTIVA" && a.EndDate <= now)
                .ToListAsync();
    }
}
