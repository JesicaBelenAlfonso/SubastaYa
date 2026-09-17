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

            // Precisión decimal para no truncar centavos.
            builder.Property(w => w.TotalBalance).HasPrecision(12, 2);
            builder.Property(w => w.HeldBalance).HasPrecision(12, 2);

            // Optimistic locking: la columna de versión la maneja SQL Server.
            builder.Property(w => w.RowVersion).IsRowVersion();

            // Relación 1:1 con User.
            builder.HasOne(w => w.User)
                   .WithOne()
                   .HasForeignKey<Wallet>(w => w.UserId);

            // Índice único: cada usuario tiene una sola wallet.
            builder.HasIndex(w => w.UserId).IsUnique();
        }
    }
}