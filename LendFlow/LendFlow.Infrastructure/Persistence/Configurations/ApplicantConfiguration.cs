using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LendFlow.Domain.Entities;

namespace LendFlow.Infrastructure.Persistence.Configurations;

public class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
{
    public void Configure(EntityTypeBuilder<Applicant> builder)
    {
        builder.ToTable("Applicants");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.IdNumber)
            .HasMaxLength(255)
            .IsRequired(); // Encrypted at rest

        builder.Property(a => a.PhoneNumber)
            .HasMaxLength(255)
            .IsRequired(); // Encrypted at rest

        builder.Property(a => a.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.EmploymentStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.MonthlyIncome)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(a => a.MonthlyExpenses)
            .HasColumnType("decimal(18,4)")
            .IsRequired();
            
        builder.Property(a => a.CreatedBy)
            .HasMaxLength(255);
            
        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(255);
    }
}
