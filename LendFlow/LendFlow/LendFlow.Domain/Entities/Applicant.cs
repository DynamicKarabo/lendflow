using System;
using LendFlow.Domain.Common;
using LendFlow.Domain.Common.Attributes;
using LendFlow.Domain.ValueObjects;

namespace LendFlow.Domain.Entities;

public class Applicant : BaseAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    [Encrypted]
    public string IdNumber { get; private set; } = string.Empty;

    [Encrypted]
    public string PhoneNumber { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string EmploymentStatus { get; private set; } = string.Empty;
    public decimal MonthlyIncome { get; private set; }
    public decimal MonthlyExpenses { get; private set; }

    private Applicant() { }

    public static Applicant Create(
        Guid tenantId,
        string firstName,
        string lastName,
        string idNumber,
        string phoneNumber,
        string email,
        DateOnly dateOfBirth,
        string employmentStatus,
        decimal monthlyIncome,
        decimal monthlyExpenses)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        
        if (age < 18) throw new InvalidOperationException("Applicant must be 18 years or older.");

        var saId = SouthAfricanIdNumber.Create(idNumber, dateOfBirth);

        return new Applicant
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            IdNumber = saId.Value,
            PhoneNumber = phoneNumber,
            Email = email,
            DateOfBirth = dateOfBirth,
            EmploymentStatus = employmentStatus,
            MonthlyIncome = monthlyIncome,
            MonthlyExpenses = monthlyExpenses
        };
    }
}
