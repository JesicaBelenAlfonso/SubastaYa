using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubastaYa.Infrastructure.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(w => w.Id);

            // Precisión decimal: sin esto, EF Core usa un default
            // que puede truncar centavos en algunos motores.
            builder.Property(w => w.TotalBalance).HasPrecision(12, 2);
            builder.Property(w => w.HeldBalance).HasPrecision(12, 2);

            // Esto es EL detalle técnico de Optimistic Locking.
            // Le dice a EF Core: "esta columna la maneja SQL Server solo,
            // y usala para saber si alguien más modificó la fila
            // entre que la leí y la quiero guardar".
            builder.Property(w => w.RowVersion).IsRowVersion();

            // La relación 1:1 con User.
            builder.HasOne(w => w.User)
                   .WithOne()
                   .HasForeignKey<Wallet>(w => w.UserId);

            // Este índice único es lo que IMPIDE que un mismo User
            // termine con dos Wallets. Sin esto, la FK sola permitiría
            // varias filas con el mismo UserId.
            builder.HasIndex(w => w.UserId).IsUnique();
        }
    }
}