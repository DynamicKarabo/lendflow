namespace LendFlow.Application.Common.Interfaces;

public interface IKycProvider
{
    Task<KycResult> VerifyAsync(string idNumber, string firstName, string lastName, DateTime dateOfBirth, CancellationToken ct = default);
}

public record KycResult(
    bool IsVerified,
    string? ErrorMessage = null,
    string? FullName = null,
    DateTime? DateOfBirth = null,
    string? Gender = null,
    string? Citizenship = null
);
