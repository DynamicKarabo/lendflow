using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LendFlow.Domain.Entities;

namespace LendFlow.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.PreviousState)
            .HasMaxLength(100);

        builder.Property(a => a.NewState)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.PerformedBy)
            .HasMaxLength(255);

        builder.Property(a => a.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(a => new { a.EntityId, a.EntityType });
        builder.HasIndex(a => a.OccurredAt);
    }
}
