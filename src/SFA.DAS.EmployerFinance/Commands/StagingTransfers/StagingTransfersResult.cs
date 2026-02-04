namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class StageTransfersResult
{
    public bool IsSuccess { get; set; }
    public List<long> InsertedTransferIds { get; set; } = new();
    public List<long> ConflictingTransferIds { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();
}
