using SubastaYa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubastaYa.Infrastructure.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(w => w.Id);

            builder.Property(w => w.TotalBalance).HasPrecision(12, 2);
            builder.Property(w => w.HeldBalance).HasPrecision(12, 2);

            builder.Property(w => w.RowVersion).IsRowVersion();

            builder.HasOne(w => w.User)
                   .WithOne()
                   .HasForeignKey<Wallet>(w => w.UserId);

            builder.HasIndex(w => w.UserId).IsUnique();

            // Datos semilla: 4 wallets con saldos iniciales
            builder.HasData(
                new Wallet { Id = 1, UserId = 1, TotalBalance = 0m, HeldBalance = 0m },
                new Wallet { Id = 2, UserId = 2, TotalBalance = 150000m, HeldBalance = 45000m },
                new Wallet { Id = 3, UserId = 3, TotalBalance = 200000m, HeldBalance = 0m },
                new Wallet { Id = 4, UserId = 4, TotalBalance = 500m, HeldBalance = 0m }
            );
        }
    }
}