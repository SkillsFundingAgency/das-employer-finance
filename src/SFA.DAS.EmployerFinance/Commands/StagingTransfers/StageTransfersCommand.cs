using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class StageTransfersCommand : IRequest<BulkTransferStagingResponse>
{
    public List<TransferStaging> Transfers { get; set; }

    public string CorrelationId { get; set; }
}
