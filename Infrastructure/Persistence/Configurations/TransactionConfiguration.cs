using SubastaYa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubastaYa.Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(t => t.Amount)
                   .HasPrecision(12, 2);

            builder.HasOne<Wallet>()
                   .WithMany()
                   .HasForeignKey(t => t.WalletId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.WalletId);

            // Datos semilla: Transacciones que respaldan el libro mayor
            // y reflejan el saldo retenido de $45.000 del comprador1
            builder.HasData(
                new Transaction
                {
                    Id = 1,
                    Type = "Depósito Inicial",
                    Amount = 150000m,
                    WalletId = 2
                },
                new Transaction
                {
                    Id = 2,
                    Type = "Retención de Puja",
                    Amount = 45000m,
                    WalletId = 2
                },
                new Transaction
                {
                    Id = 3,
                    Type = "Depósito Inicial",
                    Amount = 200000m,
                    WalletId = 3
                },
                new Transaction
                {
                    Id = 4,
                    Type = "Depósito Inicial",
                    Amount = 500m,
                    WalletId = 4
                }
            );
        }
    }
}