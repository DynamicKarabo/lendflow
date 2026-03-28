using LendFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace LendFlow.Infrastructure.Services.Stubs;

public class StubKycProvider : IKycProvider
{
    private readonly ILogger<StubKycProvider> _logger;

    public StubKycProvider(ILogger<StubKycProvider> logger)
    {
        _logger = logger;
    }

    public Task<KycResult> VerifyAsync(string idNumber, string firstName, string lastName, DateTime dateOfBirth, CancellationToken ct = default)
    {
        _logger.LogInformation("KYC verification stub called for {IdNumber} - returning verified", idNumber[..Math.Min(6, idNumber.Length)] + "***");
        
        return Task.FromResult(new KycResult(
            IsVerified: true,
            FullName: $"{firstName} {lastName}",
            DateOfBirth: dateOfBirth,
            Gender: "Unknown",
            Citizenship: "SA"
        ));
    }
}
