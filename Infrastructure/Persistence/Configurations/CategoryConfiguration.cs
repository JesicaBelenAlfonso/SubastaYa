using SubastaYa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubastaYa.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                   .HasColumnName("Id");

            builder.Property(c => c.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(c => c.UrlIcono)
                   .HasMaxLength(500);

            // Datos semilla: 4 categorías base
            builder.HasData(
                new Category { Id = 1, Name = "Tecnología", UrlIcono = "https://i.postimg.cc/4PmHRyk5/tecnology.png" },
                new Category { Id = 2, Name = "Coleccionables", UrlIcono = "https://i.postimg.cc/jPGHx8fZ/collectibles.png" },
                new Category { Id = 3, Name = "Indumentaria", UrlIcono = "https://i.postimg.cc/9NfL4bZY/clothing.png" },
                new Category { Id = 4, Name = "Vehículos", UrlIcono = "https://i.postimg.cc/c2LjT6qL/vehicles.png" }
            );
        }
    }
}