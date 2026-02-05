namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class BulkTransferStagingResponse
{
    public int InsertedCount { get; set; }
    public List<long> TransferIds { get; set; } = new();

    public List<string>? ValidationErrors { get; set; }
    public List<long>? ConflictingTransferIds { get; set; }

    public bool HasValidationErrors { get; set; }
    public bool HasConflicts { get; set; }
    public bool IsSuccess => !HasValidationErrors && !HasConflicts;
}