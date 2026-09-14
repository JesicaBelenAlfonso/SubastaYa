using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
namespace Infraestructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Wallet> Wallets => Set<Wallet>();

        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Auction> Auctions => Set<Auction>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Bid> Bids => Set<Bid>();
        public DbSet<Audit> Audits => Set<Audit>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
