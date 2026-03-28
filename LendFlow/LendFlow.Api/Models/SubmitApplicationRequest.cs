namespace LendFlow.Api.Models;

public record SubmitApplicationRequest(
    Guid ApplicantId,
    decimal Amount,
    int TermMonths,
    string Purpose,
    string IdempotencyKey
);
