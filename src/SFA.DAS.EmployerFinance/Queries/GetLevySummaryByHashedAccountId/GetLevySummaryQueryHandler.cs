using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public class GetLevySummaryQueryHandler(IDasLevyService dasLevyService, IEncodingService encodingService) : IRequestHandler<GetLevySummaryQuery, GetLevySummaryQueryResult>
{
    public async Task<GetLevySummaryQueryResult> Handle(GetLevySummaryQuery request, CancellationToken cancellationToken)
    {
        var accountId = encodingService.Decode(request.HashedAccountId, EncodingType.AccountId);

        var currentAccountBalance = await dasLevyService.GetAccountBalance(accountId);

        return new GetLevySummaryQueryResult
        {
            Summary = new LevySummary
            {
                CurrentLevyFunds = currentAccountBalance
            }
        };
    }
}