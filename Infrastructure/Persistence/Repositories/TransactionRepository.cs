

using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _ctx;

        public TransactionRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _ctx.Transactions.AddAsync(transaction);
        }

        public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId)
        {
            return await _ctx.Transactions
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<bool> ExistsByWalletIdAsync(int walletId)
        {
            return await _ctx.Transactions.AnyAsync(t => t.WalletId == walletId);
        }

        public async Task<Transaction?> GetByIdAsync(int walletId, int id)
        {
            return await _ctx.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.WalletId == walletId);
        }
    }
}