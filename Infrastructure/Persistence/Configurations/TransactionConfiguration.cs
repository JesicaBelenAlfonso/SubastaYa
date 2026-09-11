using Domain.Entities;
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

            // El ledger apunta a la billetera que le dio origen.
            // OnDelete Restrict: no se puede borrar una billetera con
            // movimientos — el historial contable es inmutable.
            builder.HasOne<Wallet>()
                   .WithMany()
                   .HasForeignKey(t => t.WalletId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.WalletId);
        }
    }
}