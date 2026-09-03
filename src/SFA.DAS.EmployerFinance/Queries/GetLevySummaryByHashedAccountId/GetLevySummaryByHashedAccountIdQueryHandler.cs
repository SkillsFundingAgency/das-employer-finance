using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public class GetLevySummaryByHashedAccountIdQueryHandler(IDasLevyService dasLevyService,
    IDasLevyRepository dasLevyRepository,
    IEncodingService encodingService) : IRequestHandler<GetLevySummaryByHashedAccountIdQuery, GetLevySummaryByHashedAccountIdQueryResult>
{
    private const int TwelveMonths = 12;

    public async Task<GetLevySummaryByHashedAccountIdQueryResult> Handle(GetLevySummaryByHashedAccountIdQuery request, CancellationToken cancellationToken)
    {
        var accountId = encodingService.Decode(request.HashedAccountId, EncodingType.AccountId);

        var currentAccountBalance = await dasLevyService.GetAccountBalance(accountId);
        var levyDeclarations = await dasLevyRepository.GetAccountLevyDeclaredForPreviousMonths(accountId, TwelveMonths);

        return new GetLevySummaryByHashedAccountIdQueryResult
        {
            Summary = new LevySummary
            {
                CurrentLevyFunds = currentAccountBalance,
                TotalLevyDeclaredLast12Months = levyDeclarations.Sum(x => x.TotalAmount)
            }
        };
    }
}