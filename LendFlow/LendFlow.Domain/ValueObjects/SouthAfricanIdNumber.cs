using System;
using System.Text.RegularExpressions;

namespace LendFlow.Domain.ValueObjects;

public class SouthAfricanIdNumber
{
    public string Value { get; }

    private SouthAfricanIdNumber(string value)
    {
        Value = value;
    }

    public static SouthAfricanIdNumber Create(string idNumber, DateOnly applicantDob)
    {
        if (string.IsNullOrWhiteSpace(idNumber))
            throw new ArgumentException("ID number cannot be empty.");

        if (idNumber.Length != 13 || !Regex.IsMatch(idNumber, @"^\d{13}$"))
            throw new ArgumentException("ID number must be exactly 13 digits.");

        // Compare DOB
        string dobPart = idNumber.Substring(0, 6);
        string expectedDobPart = applicantDob.ToString("yyMMdd");

        if (dobPart != expectedDobPart)
            throw new ArgumentException("ID number does not match the provided date of birth.");

        // Gender digit (6-9)
        // Citizenship digit (10)
        char citizenshipDigit = idNumber[10];
        if (citizenshipDigit != '0' && citizenshipDigit != '1')
            throw new ArgumentException("ID number citizenship digit must be '0' or '1'.");

        // Luhn check
        if (!IsValidLuhn(idNumber))
            throw new ArgumentException("ID number fails Luhn validation.");

        return new SouthAfricanIdNumber(idNumber);
    }

    private static bool IsValidLuhn(string number)
    {
        int sum = 0;
        bool alternate = false;
        
        for (int i = number.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(number.Substring(i, 1));

            if (alternate)
            {
                n *= 2;
                if (n > 9)
                {
                    n = (n % 10) + 1;
                }
            }

            sum += n;
            alternate = !alternate;
        }

        return (sum % 10 == 0);
    }

    public override bool Equals(object? obj)
    {
        return obj is SouthAfricanIdNumber other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
