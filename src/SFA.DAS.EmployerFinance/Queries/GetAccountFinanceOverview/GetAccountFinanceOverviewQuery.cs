namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewQuery : IRequest<GetAccountFinanceOverviewResponse>
{
    public long AccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}