using SubastaYa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubastaYa.Infrastructure.Persistence.Configurations
{
    public class AuditConfiguration : IEntityTypeConfiguration<Audit>
    {
        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Entity)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(a => a.Action)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(a => a.DetalleJson)
                   .IsRequired();

            builder.HasIndex(a => new { a.Entity, a.EntityId });
        }
    }
}
