#nullable enable
namespace SFA.DAS.EmployerFinance.Commands.TransactionLineStaging;

public class TransactionLineStagingResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public int InsertedCount { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public bool HasValidationErrors { get; set; }
}
