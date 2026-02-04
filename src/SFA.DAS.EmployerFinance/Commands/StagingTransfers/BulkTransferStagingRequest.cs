using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class BulkTransferStagingRequest
{
    public List<TransferStagingDto> Transfers { get; set; } = new();
}
