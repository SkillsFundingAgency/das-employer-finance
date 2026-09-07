using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;

public class GetLevySummaryByAccountIdQueryHandler(IDasLevyService dasLevyService,
    IDasLevyRepository dasLevyRepository) : IRequestHandler<GetLevySummaryByAccountIdQuery, GetLevySummaryByAccountIdQueryResult>
{
    private const int TwelveMonths = 12;

    public async Task<GetLevySummaryByAccountIdQueryResult> Handle(GetLevySummaryByAccountIdQuery request, CancellationToken cancellationToken)
    {
        var currentAccountBalance = await dasLevyService.GetAccountBalance(request.AccountId);
        var levyDeclarations = await dasLevyRepository.GetAccountLevyDeclaredForPreviousMonths(request.AccountId, TwelveMonths);
        var levySpent = await dasLevyRepository.GetAccountLevySpentForPreviousMonths(request.AccountId, TwelveMonths);

        return new GetLevySummaryByAccountIdQueryResult
        {
            Summary = new LevySummary
            {
                CurrentLevyFunds = currentAccountBalance,
                TotalLevyDeclaredLast12Months = levyDeclarations.Sum(x => x.TotalAmount),
                TotalLevySpentLast12Months = levySpent.Sum(x => x.TotalAmount)
            }
        };
    }
}