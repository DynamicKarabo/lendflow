using System;
namespace LendFlow.Tests.Testing;

public static class TestData
{
    public static string CreateValidSaId(DateOnly dob, int sequence = 5000, char citizenshipDigit = '0', char otherDigit = '8')
    {
        var dobPart = dob.ToString("yyMMdd");
        var sequencePart = sequence.ToString("D4");
        var withoutCheck = $"{dobPart}{sequencePart}{citizenshipDigit}{otherDigit}";
        var checkDigit = CalculateLuhnCheckDigit(withoutCheck);
        return withoutCheck + checkDigit;
    }

    private static int CalculateLuhnCheckDigit(string number)
    {
        int sum = 0;
        bool alternate = true;

        for (int i = number.Length - 1; i >= 0; i--)
        {
            int n = number[i] - '0';
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

        var mod = sum % 10;
        return mod == 0 ? 0 : 10 - mod;
    }
}
