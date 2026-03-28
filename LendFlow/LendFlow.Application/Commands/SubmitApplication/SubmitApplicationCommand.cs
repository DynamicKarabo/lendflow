using System;
using MediatR;

namespace LendFlow.Application.Commands.SubmitApplication;

public record SubmitApplicationResult(Guid ApplicationId);

public record SubmitApplicationCommand(
    Guid TenantId,
    Guid ApplicantId,
    decimal RequestedAmount,
    int RequestedTermMonths,
    string Purpose,
    string IdempotencyKey
) : IRequest<SubmitApplicationResult>;
