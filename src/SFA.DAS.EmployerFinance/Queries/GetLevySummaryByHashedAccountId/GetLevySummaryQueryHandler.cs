using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public class GetLevySummaryQueryHandler(IDasLevyService dasLevyService,
    IDasLevyRepository dasLevyRepository,
    IEncodingService encodingService) : IRequestHandler<GetLevySummaryQuery, GetLevySummaryQueryResult>
{
    private const int TwelveMonths = 12;

    public async Task<GetLevySummaryQueryResult> Handle(GetLevySummaryQuery request, CancellationToken cancellationToken)
    {
        var accountId = encodingService.Decode(request.HashedAccountId, EncodingType.AccountId);

        var currentAccountBalance = await dasLevyService.GetAccountBalance(accountId);
        var levyDeclarations = await dasLevyRepository.GetAccountLevyDeclarationsForPreviousMonths(accountId, TwelveMonths);

        return new GetLevySummaryQueryResult
        {
            Summary = new LevySummary
            {
                CurrentLevyFunds = currentAccountBalance,
                TotalLevyDeclaredLast12Months = levyDeclarations.Sum(x => x.TotalAmount)
            }
        };
    }
}