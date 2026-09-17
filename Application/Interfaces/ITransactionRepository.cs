using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId);
        Task<Transaction?> GetByIdAsync(int walletId, int id);
        Task<bool> ExistsByWalletIdAsync(int walletId);
    }
}