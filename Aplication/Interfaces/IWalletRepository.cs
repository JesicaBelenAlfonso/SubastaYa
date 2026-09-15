using   SubastaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.Interfaces
{
    public interface IWalletRepository
    {
        Task AddAsync(Wallet wallet);
        Task<Wallet?> GetByUserIdAsync(int userId);
        void Delete(Wallet wallet);
    }
}
