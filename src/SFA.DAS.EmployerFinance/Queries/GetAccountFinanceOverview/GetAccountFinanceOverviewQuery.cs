using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewQuery : IRequest<GetAccountFinanceOverviewResponse>
{
    public long AccountId { get; set; }
}