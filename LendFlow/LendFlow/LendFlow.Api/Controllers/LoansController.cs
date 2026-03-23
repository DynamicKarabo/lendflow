using LendFlow.Api.Models;
using LendFlow.Application.Commands.DisburseLoan;
using LendFlow.Application.Commands.RecordRepayment;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.Queries.GetLoan;
using LendFlow.Application.Queries.GetLoans;
using LendFlow.Application.Queries.GetRepayments;
using LendFlow.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/loans")]
public class LoansController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentTenantService _tenantService;

    public LoansController(IMediator mediator, ICurrentTenantService tenantService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(GetLoansResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListLoans(
        [FromQuery] LoanStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetLoansQuery(status, pageNumber, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,underwriter")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoan(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLoanQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("{id}/disburse")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(DisburseLoanResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisburseLoan(
        Guid id,
        [FromBody] DisburseLoanRequest request,
        CancellationToken ct)
    {
        var command = new DisburseLoanCommand(
            _tenantService.TenantId,
            id,
            request.Method,
            request.AccountNumber,
            request.BankCode,
            request.IdempotencyKey
        );
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("{id}/repayments")]
    [Authorize(Roles = "admin,underwriter")]
    [ProducesResponseType(typeof(List<RepaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRepayments(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRepaymentsQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("{id}/repayments/pay")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(RecordRepaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordRepayment(
        Guid id,
        [FromBody] RecordRepaymentRequest request,
        CancellationToken ct)
    {
        var command = new RecordRepaymentCommand(
            _tenantService.TenantId,
            id,
            request.Amount,
            request.PaymentReference,
            request.IdempotencyKey
        );
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
