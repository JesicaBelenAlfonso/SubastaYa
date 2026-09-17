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
        }
    }
}
