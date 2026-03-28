using MediatR;

namespace LendFlow.Application.Commands.AssessCredit;

public record AssessCreditCommand(
    Guid TenantId,
    Guid ApplicationId
) : IRequest<AssessCreditResult>;
