namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class StageTransfersResponse
{
    public bool IsSuccess { get; set; }
    public List<long> InsertedTransferIds { get; set; } = new();
    public List<long> ConflictingTransferIds { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();
    public bool HasConflicts { get; set; }
    public bool HasValidationErrors { get; set; }
}
