using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LendFlow.Domain.Entities;

namespace LendFlow.Infrastructure.Persistence.Configurations;

public class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("LoanApplications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.RequestedAmount)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(a => a.Purpose)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.RiskBand)
            .HasMaxLength(20);

        builder.Property(a => a.DecisionReason)
            .HasMaxLength(500);

        builder.Property(a => a.IdempotencyKey)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(255);
            
        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(255);

        builder.HasIndex(a => new { a.TenantId, a.IdempotencyKey })
            .IsUnique();

        builder.HasIndex(a => new { a.TenantId, a.Status });

        builder.HasIndex(a => new { a.TenantId, a.ApplicantId });
    }
}
