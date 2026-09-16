using SubastaYa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubastaYa.Infrastructure.Persistence.Configurations
{
    public class BidConfiguration : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Amount)
                   .HasPrecision(12, 2);

            builder.HasOne<Auction>()
                   .WithMany()
                   .HasForeignKey(b => b.AuctionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => b.AuctionId);
            builder.HasIndex(b => b.BuyerId);

            // Datos semilla: Historial ofertas en Subasta 1 (estándar)
            // El comprador1 lidera con $45,000 y hay una oferta menor anterior
            builder.HasData(
                new Bid
                {
                    Id = 1,
                    AuctionId = 1,
                    BuyerId = 2,
                    Amount = 45000m
                },
                new Bid
                {
                    Id = 2,
                    AuctionId = 1,
                    BuyerId = 3,
                    Amount = 10000m
                }
            );
        }
    }
}