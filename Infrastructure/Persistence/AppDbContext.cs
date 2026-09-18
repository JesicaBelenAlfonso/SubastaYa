using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SubastaYa.Domain.Entities;
		  

        
namespace SubastaYa.Infrastructure.Persistence
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
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Auction> Auctions => Set<Auction>();       
        public DbSet<Bid> Bids => Set<Bid>();
        public DbSet<Audit> Audits => Set<Audit>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // SQL Server no preserva DateTime.Kind: la app guarda todo en UTC, así
            // que al leer se normaliza a Kind=Utc para que el JSON salga con "Z" y
            // el front no la interprete como hora local (corrimiento de zonas).
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => v,
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(new ValueConverter<DateTime?, DateTime?>(
                            v => v,
                            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v));
                }
            }
        }
    }
}
