using SubastaYa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Índice UNIQUE de email a nivel base de datos.
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.PasswordHash)
                   .HasMaxLength(255)
                   .IsRequired();
        }
    }
}
