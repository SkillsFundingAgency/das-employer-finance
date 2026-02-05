using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class BulkTransferStagingRequest
{
    public List<TransferStaging> Transfers { get; set; } = new();
}
