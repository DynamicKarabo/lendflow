using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LendFlow.Domain.Entities;

namespace LendFlow.Infrastructure.Persistence.Configurations;

public class RepaymentConfiguration : IEntityTypeConfiguration<Repayment>
{
    public void Configure(EntityTypeBuilder<Repayment> builder)
    {
        builder.ToTable("Repayments");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.AmountDue)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(r => r.AmountPaid)
            .HasColumnType("decimal(18,4)");

        builder.Property(r => r.DueDate)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.PaymentReference)
            .HasMaxLength(255);

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(255);
            
        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(255);

        builder.HasIndex(r => new { r.LoanId, r.Status });
        builder.HasIndex(r => new { r.DueDate, r.Status });
    }
}
