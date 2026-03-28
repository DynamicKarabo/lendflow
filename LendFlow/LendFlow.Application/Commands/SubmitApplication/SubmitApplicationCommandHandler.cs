using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Exceptions;
using MediatR;

namespace LendFlow.Application.Commands.SubmitApplication;

public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, SubmitApplicationResult>
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly IAppDbContext _dbContext;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public SubmitApplicationCommandHandler(
        IIdempotencyService idempotencyService, 
        IAppDbContext dbContext,
        IDomainEventDispatcher eventDispatcher)
    {
        _idempotencyService = idempotencyService;
        _dbContext = dbContext;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<SubmitApplicationResult> Handle(SubmitApplicationCommand command, CancellationToken ct)
    {
        var storedResult = await _idempotencyService.GetStoredResultAsync(command.IdempotencyKey);
        if (storedResult != null)
        {
            return JsonSerializer.Deserialize<SubmitApplicationResult>(storedResult)!;
        }

        var applicant = await _dbContext.GetApplicantAsync(command.ApplicantId, ct);

        if (applicant == null || applicant.TenantId != command.TenantId)
        {
            throw new NotFoundException(nameof(Applicant), command.ApplicantId);
        }

        var loanApplication = LoanApplication.Create(
            command.TenantId,
            command.ApplicantId,
            command.RequestedAmount,
            command.RequestedTermMonths,
            command.Purpose,
            command.IdempotencyKey
        );

        loanApplication.Submit();

        _dbContext.AddLoanApplication(loanApplication);
        await _dbContext.SaveChangesAsync(ct);

        await _eventDispatcher.DispatchManyAsync(loanApplication.DomainEvents, ct);
        loanApplication.ClearDomainEvents();

        var result = new SubmitApplicationResult(loanApplication.Id);

        await _idempotencyService.StoreResultAsync(command.IdempotencyKey, JsonSerializer.Serialize(result));

        return result;
    }
}
