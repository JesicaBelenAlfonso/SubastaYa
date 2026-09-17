using SubastaYa.Domain.Entities;
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
                   .HasConversion(
                       v => v.ToString().ToUpperInvariant(),
                       v => Enum.Parse<AuctionStatus>(v, true))
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
            builder.Property(a => a.RowVersion).IsRowVersion();

            // Datos semilla: 5 subastas base con diferentes estados y fechas
            var baseDate = new DateTime(2026, 9, 16, 14, 0, 0);

            builder.HasData(
                new Auction
                {
                    Id = 1,
                    Title = "Laptop Gamer RTX 4080",
                    Descripcion = "Subasta estándar de tecnología",
                    UrlImagen = "https://i.postimg.cc/4PmHRyk5/auction1.png",
                    BasePrice = 1000m,
                    MinIncrement = 50m,
                    CategoryId = 1,
                    SellerId = 1,
                    Status = "Active",
                    StartDate = baseDate,
                    EndDate = baseDate.AddMinutes(25)
                },
                new Auction
                {
                    Id = 2,
                    Title = "Lote de Figuras Coleccionables",
                    Descripcion = "Subasta crítica de tecnología",
                    UrlImagen = "https://i.postimg.cc/jPGHx8fZ/auction2.png",
                    BasePrice = 200m,
                    MinIncrement = 20m,
                    CategoryId = 2,
                    SellerId = 1,
                    Status = "Active",
                    StartDate = baseDate,
                    EndDate = baseDate.AddMinutes(1.5)
                },
                new Auction
                {
                    Id = 3,
                    Title = "Camiseta Firmada Edición Limitada",
                    Descripcion = "Subasta próxima de indumentaria",
                    UrlImagen = "https://i.postimg.cc/9NfL4bZY/auction3.png",
                    BasePrice = 50m,
                    MinIncrement = 10m,
                    CategoryId = 3,
                    SellerId = 1,
                    Status = "Pending",
                    StartDate = baseDate.AddHours(48),
                    EndDate = baseDate.AddHours(52)
                },
                new Auction
                {
                    Id = 4,
                    Title = "Auto Clásico 1998",
                    Descripcion = "Subasta vencida con ganador",
                    UrlImagen = "https://i.postimg.cc/c2LjT6qL/auction4.png",
                    BasePrice = 5000m,
                    MinIncrement = 500m,
                    CategoryId = 4,
                    SellerId = 1,
                    Status = "Closed",
                    StartDate = baseDate.AddDays(-10),
                    EndDate = baseDate.AddHours(-2)
                },
                new Auction
                {
                    Id = 5,
                    Title = "Teléfono Inteligente Sin Uso",
                    Descripcion = "Subasta vencida desierta",
                    UrlImagen = "https://i.postimg.cc/8sdfg/auction5.png",
                    BasePrice = 300m,
                    MinIncrement = 30m,
                    CategoryId = 1,
                    SellerId = 1,
                    Status = "Deserted",
                    StartDate = baseDate.AddDays(-10),
                    EndDate = baseDate.AddHours(-5)
                }
            );
        }
    }
}