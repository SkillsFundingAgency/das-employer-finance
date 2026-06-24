namespace SFA.DAS.EmployerFinance.Queries.FindEmployerAccountExpiredFunds;

public class FindEmployerAccountExpiredFundsQuery : IRequest<FindEmployerAccountExpiredFundsResponse>
{
    public string HashedAccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}