using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces;

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

        public async Task<Auction?> GetByIdAsync(int id)
            => await _ctx.Auctions.FirstOrDefaultAsync(a => a.Id == id);

        public void Delete(Auction auction)
            => _ctx.Auctions.Remove(auction);
    }
}
