using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using MediatR;

namespace LendFlow.Application.Commands.CreateApplicant;

public class CreateApplicantCommandHandler : IRequestHandler<CreateApplicantCommand, CreateApplicantResult>
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly IAppDbContext _dbContext;

    public CreateApplicantCommandHandler(IIdempotencyService idempotencyService, IAppDbContext dbContext)
    {
        _idempotencyService = idempotencyService;
        _dbContext = dbContext;
    }

    public async Task<CreateApplicantResult> Handle(CreateApplicantCommand command, CancellationToken ct)
    {
        var storedResult = await _idempotencyService.GetStoredResultAsync(command.IdempotencyKey);
        if (storedResult != null)
        {
            return JsonSerializer.Deserialize<CreateApplicantResult>(storedResult)!;
        }

        var applicant = Applicant.Create(
            command.TenantId,
            command.FirstName,
            command.LastName,
            command.IdNumber,
            command.PhoneNumber,
            command.Email,
            command.DateOfBirth,
            command.EmploymentStatus,
            command.MonthlyIncome,
            command.MonthlyExpenses
        );

        _dbContext.AddApplicant(applicant);
        await _dbContext.SaveChangesAsync(ct);

        var result = new CreateApplicantResult(applicant.Id);

        await _idempotencyService.StoreResultAsync(command.IdempotencyKey, JsonSerializer.Serialize(result));

        return result;
    }
}
