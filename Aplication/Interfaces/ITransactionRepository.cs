using Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId);
    }
}