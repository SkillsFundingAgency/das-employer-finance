namespace SFA.DAS.EmployerFinance.Commands.TransferStagedToOperational;

public class TransferStagedToOperationalResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public int ProcessedCount { get; set; }
    public bool HasValidationErrors { get; set; }
    public List<string> ValidationErrors { get; set; } = [];
}
