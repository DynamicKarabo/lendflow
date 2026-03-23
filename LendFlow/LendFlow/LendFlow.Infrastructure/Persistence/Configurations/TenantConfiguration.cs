using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LendFlow.Domain.Entities;

namespace LendFlow.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.ApiKeyHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(t => t.ApiKeyHash)
            .IsUnique();
    }
}
