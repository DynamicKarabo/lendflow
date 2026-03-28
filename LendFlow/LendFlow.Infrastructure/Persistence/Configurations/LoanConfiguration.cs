using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LendFlow.Domain.Entities;

namespace LendFlow.Infrastructure.Persistence.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.PrincipalAmount)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(l => l.InterestRate)
            .HasColumnType("decimal(8,4)")
            .IsRequired();

        builder.Property(l => l.RepaymentFrequency)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.MaturityDate)
            .IsRequired();

        builder.Property(l => l.OutstandingBalance)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.CreatedBy)
            .HasMaxLength(255);
            
        builder.Property(l => l.UpdatedBy)
            .HasMaxLength(255);

        builder.HasIndex(l => new { l.TenantId, l.Status });
        builder.HasIndex(l => l.ApplicationId).IsUnique();
        builder.HasIndex(l => new { l.TenantId, l.ApplicantId });
    }
}
