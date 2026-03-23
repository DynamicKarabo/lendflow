using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LendFlow.Domain.Entities;

namespace LendFlow.Infrastructure.Persistence.Configurations;

public class CreditAssessmentConfiguration : IEntityTypeConfiguration<CreditAssessment>
{
    public void Configure(EntityTypeBuilder<CreditAssessment> builder)
    {
        builder.ToTable("CreditAssessments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Score)
            .IsRequired();

        builder.Property(c => c.RiskBand)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.FactorBreakdown)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(c => c.ApplicationId).IsUnique();
    }
}
