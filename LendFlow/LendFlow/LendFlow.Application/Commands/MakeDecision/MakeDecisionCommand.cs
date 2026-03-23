using MediatR;

namespace LendFlow.Application.Commands.MakeDecision;

public record MakeDecisionCommand(
    Guid TenantId,
    Guid ApplicationId,
    string Decision,
    string Reason
) : IRequest<MakeDecisionResult>;
