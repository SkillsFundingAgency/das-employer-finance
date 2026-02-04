namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class BulkTransferStagingResponse
{
    public int InsertedCount { get; set; }
    public List<long> TransferIds { get; set; } = new();

    public List<string>? ValidationErrors { get; set; }
    public List<long>? ConflictingTransferIds { get; set; }

    public bool HasValidationErrors => ValidationErrors?.Any() == true;
    public bool HasConflicts => ConflictingTransferIds?.Any() == true;
    public bool IsSuccess => !HasValidationErrors && !HasConflicts;
}