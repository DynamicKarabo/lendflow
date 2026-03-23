using LendFlow.Api.Models;
using LendFlow.Application.Commands.SubmitApplication;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.Queries.GetLoanApplication;
using LendFlow.Application.Queries.GetLoanApplications;
using LendFlow.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentTenantService _tenantService;

    public ApplicationsController(IMediator mediator, ICurrentTenantService tenantService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubmitApplicationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitApplication(
        [FromBody] SubmitApplicationRequest request,
        CancellationToken ct)
    {
        var command = new SubmitApplicationCommand(
            TenantId: _tenantService.TenantId,
            ApplicantId: request.ApplicantId,
            RequestedAmount: request.Amount,
            RequestedTermMonths: request.TermMonths,
            Purpose: request.Purpose,
            IdempotencyKey: request.IdempotencyKey
        );

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetApplication), new { id = result.ApplicationId }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(LendFlow.Application.Common.Models.PagedResult<LoanApplicationListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListApplications(
        [FromQuery] LoanApplicationStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetLoanApplicationsQuery(status, pageNumber, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplication(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLoanApplicationQuery(id), ct);
        return Ok(result);
    }
}
