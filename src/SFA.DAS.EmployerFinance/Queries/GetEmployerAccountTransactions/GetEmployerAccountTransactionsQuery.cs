namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

public class GetEmployerAccountTransactionsQuery : IRequest<GetEmployerAccountTransactionsResponse>
{
    public string HashedAccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}