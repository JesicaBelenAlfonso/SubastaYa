using SubastaYa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubastaYa.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.PasswordHash)
                   .HasMaxLength(255)
                   .IsRequired();

            // Datos semilla: 4 usuarios base
            builder.HasData(
                new User { Id = 1, Email = "vendedor@test.com", Name = "Vendedor Test", PasswordHash = "placeholder_hash_1" },
                new User { Id = 2, Email = "comprador1@test.com", Name = "Comprador 1 Test", PasswordHash = "placeholder_hash_2" },
                new User { Id = 3, Email = "comprador2@test.com", Name = "Comprador 2 Test", PasswordHash = "placeholder_hash_3" },
                new User { Id = 4, Email = "sinfondos@test.com", Name = "Sin Fondos Test", PasswordHash = "placeholder_hash_4" }
            );
        }
    }
}