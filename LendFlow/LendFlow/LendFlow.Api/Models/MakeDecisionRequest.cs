namespace LendFlow.Api.Models;

public class MakeDecisionRequest
{
    public string Decision { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
