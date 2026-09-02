using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public class GetLevySummaryQueryHandler(IDasLevyService dasLevyService, IEncodingService encodingService) : IRequestHandler<GetLevySummaryQuery, GetLevySummaryResponse>
{
    public async Task<GetLevySummaryResponse> Handle(GetLevySummaryQuery request, CancellationToken cancellationToken)
    {
        var accountId = encodingService.Decode(request.HashedAccountId, EncodingType.AccountId);

        var currentAccountBalance = await dasLevyService.GetAccountBalance(accountId);

        return new GetLevySummaryResponse
        {
            Summary = new LevySummary
            {
                CurrentLevyFunds = currentAccountBalance
            }
        };
    }
}