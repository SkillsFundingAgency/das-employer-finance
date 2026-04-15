using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

namespace SFA.DAS.EmployerFinance.Queries.GetExistingTransactionLines;

public class GetExistingTransactionLinesQuery : IRequest<GetEmployerAccountTransactionsResponse>
{
    public string HashedAccountId { get; set; }
    public string PeriodEnd { get; set; }
    public int TransactionType { get; set; } = 3;
}
