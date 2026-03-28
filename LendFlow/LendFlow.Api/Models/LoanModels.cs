namespace LendFlow.Api.Models;

public class DisburseLoanRequest
{
    public string Method { get; set; } = "bank_transfer";
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
}

public class RecordRepaymentRequest
{
    public decimal Amount { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
}
