using System;
using LendFlow.Domain.ValueObjects;
using LendFlow.Tests.Testing;
using Xunit;

namespace LendFlow.Tests.Domain;

public class SouthAfricanIdNumberTests
{
    [Fact]
    public void Create_ValidId_PassesLuhnCheck()
    {
        var dob = new DateOnly(1990, 1, 15);
        var idString = TestData.CreateValidSaId(dob);
        
        var saId = SouthAfricanIdNumber.Create(idString, dob);
        
        Assert.NotNull(saId);
        Assert.Equal(idString, saId.Value);
    }

    [Fact]
    public void Create_InvalidId_FailsLuhnCheck()
    {
        var dob = new DateOnly(1990, 1, 15);
        var validId = TestData.CreateValidSaId(dob);
        
        // Invalidate Luhn
        var invalidLuhn = validId.Substring(0, 12) + (validId[12] == '0' ? "1" : "0");
        
        var ex = Assert.Throws<ArgumentException>(() => SouthAfricanIdNumber.Create(invalidLuhn, dob));
        Assert.Contains("Luhn", ex.Message);
    }

    [Fact]
    public void ExtractDateOfBirth_ReturnsCorrectDate()
    {
        var dob = new DateOnly(1990, 1, 15);
        var idString = TestData.CreateValidSaId(dob);
        var saId = SouthAfricanIdNumber.Create(idString, dob);
        
        var extractedDob = saId.ExtractDateOfBirth();
        
        Assert.Equal(dob, extractedDob);
    }

    [Fact]
    public void InvalidDateComponent_ReturnsNull_ForBadDate()
    {
        // Reflection to inject an invalid format after object creation to test ExtractDateOfBirth
        var dob = new DateOnly(1990, 1, 15);
        var idString = TestData.CreateValidSaId(dob);
        var saId = SouthAfricanIdNumber.Create(idString, dob);

        var field = typeof(SouthAfricanIdNumber).GetField("<Value>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field.SetValue(saId, "9013405000080");

        var extractedDob = saId.ExtractDateOfBirth();

        Assert.Null(extractedDob);
    }
}
