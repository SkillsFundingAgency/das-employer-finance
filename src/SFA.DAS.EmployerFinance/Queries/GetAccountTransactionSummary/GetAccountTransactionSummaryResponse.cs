using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;

public class GetAccountTransactionSummaryResponse
{
    public List<TransactionSummary> Data { get; set; }
}