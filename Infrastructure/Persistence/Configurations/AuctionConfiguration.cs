using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubastaYa.Infrastructure.Persistence.Configurations
{
    public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
    {
        public void Configure(EntityTypeBuilder<Auction> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Title)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(a => a.Descripcion)
                   .HasMaxLength(2000);

            builder.Property(a => a.UrlImagen)
                   .HasMaxLength(500);

            builder.Property(a => a.Status)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(a => a.BasePrice)
                   .HasPrecision(12, 2);

            builder.Property(a => a.MinIncrement)
                   .HasPrecision(12, 2);

            builder.HasOne<Category>()
                   .WithMany()
                   .HasForeignKey(a => a.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.CategoryId);
            builder.HasIndex(a => a.SellerId);
        }
    }
}
