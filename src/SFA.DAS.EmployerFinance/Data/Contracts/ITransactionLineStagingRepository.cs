using SFA.DAS.EmployerFinance.Models.TransactionLineStaging;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface ITransactionLineStagingRepository
{
    Task BulkInsertTransactionLinesAsync(List<TransactionLineStagingModel> transactionLines);
}