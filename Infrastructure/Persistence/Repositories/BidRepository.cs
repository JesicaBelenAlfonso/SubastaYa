using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Infrastructure.Persistence.Repositories
{
    public class BidRepository : IBidRepository
    {
        private readonly AppDbContext _ctx;

        public BidRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(Bid bid)
        {
            await _ctx.Bids.AddAsync(bid);
        }

        public async Task<decimal?> GetHighestAmountByAuctionIdAsync(int auctionId)
        {
            var hasBids = await _ctx.Bids.AnyAsync(b => b.AuctionId == auctionId);
            if (!hasBids)
                return null;

            return await _ctx.Bids
                .Where(b => b.AuctionId == auctionId)
                .MaxAsync(b => b.Amount);
        }
    }
}
