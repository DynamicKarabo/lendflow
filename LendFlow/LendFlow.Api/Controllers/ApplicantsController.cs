using LendFlow.Api.Models;
using LendFlow.Application.Commands.CreateApplicant;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.Queries.GetApplicant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/applicants")]
public class ApplicantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentTenantService _tenantService;

    public ApplicantsController(IMediator mediator, ICurrentTenantService tenantService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateApplicantResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateApplicant(
        [FromBody] CreateApplicantRequest request,
        CancellationToken ct)
    {
        var command = new CreateApplicantCommand(
            TenantId: _tenantService.TenantId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            IdNumber: request.IdNumber,
            PhoneNumber: request.PhoneNumber,
            Email: request.Email,
            DateOfBirth: request.DateOfBirth,
            EmploymentStatus: request.EmploymentStatus,
            MonthlyIncome: request.MonthlyIncome,
            MonthlyExpenses: request.MonthlyExpenses,
            IdempotencyKey: request.IdempotencyKey
        );

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetApplicant), new { id = result.ApplicantId }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplicant(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApplicantQuery(id), ct);
        return Ok(result);
    }
}
