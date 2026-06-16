using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface ITransferStagingRepository
{
    Task<List<long>> GetExistingTransferIds(List<long> transferIds);

    Task BulkInsertTransfers(List<TransferStaging> transfers);
}
