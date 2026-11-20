using SFA.DAS.EmployerFinance.Api.Extensions;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetExpiredFundsByAccountId;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class ExpiredLevyOrchestrator(IMediator mediator, ILogger<ExpiredLevyOrchestrator> logger)
{
    public async Task<ExpiredLevyResponse> GetExpiredFunds(long accountId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting expired funds for accountId {AccountId}", accountId);

        var result = await mediator.Send(new GetExpiredFundsByAccountIdQuery(accountId), cancellationToken);

        return new ExpiredLevyResponse
        {
            LongTerm = result.LongTermExpiredFunds.ToCalendarPeriodAmounts(),
            ShortTerm = result.ShortTermExpiredFunds.ToCalendarPeriodAmounts()
        };
    }
}
