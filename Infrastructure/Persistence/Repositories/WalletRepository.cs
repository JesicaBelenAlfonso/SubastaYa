using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces;
using System;
using System.Collections.Generic;

namespace SubastaYa.Infrastructure.Persistence.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _ctx;

        public WalletRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(Wallet wallet)
        {
            await _ctx.Wallets.AddAsync(wallet);
            // Igual que en UserRepository: NO se llama SaveChangesAsync acá.
        }

        public async Task<Wallet?> GetByUserIdAsync(int userId)
        {
            return await _ctx.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        }
    }
}