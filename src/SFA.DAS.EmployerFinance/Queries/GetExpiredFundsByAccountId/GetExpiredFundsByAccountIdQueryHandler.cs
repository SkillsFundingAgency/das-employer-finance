using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;

namespace SFA.DAS.EmployerFinance.Queries.GetExpiredFundsByAccountId;

public class GetExpiredFundsByAccountIdQueryHandler(IExpiredFundsRepository expiredFundsRepository)
    : IRequestHandler<GetExpiredFundsByAccountIdQuery, GetExpiredFundsByAccountIdQueryResult>
{
    private const byte LongTermTransactionType = 5;  // ExpiredFund (24-month)
    private const byte ShortTermTransactionType = 6; // ShortExpiredFund (12-month)

    public async Task<GetExpiredFundsByAccountIdQueryResult> Handle(GetExpiredFundsByAccountIdQuery request, CancellationToken cancellationToken)
    {
        // Split by type before building the dictionaries - both types can exist for the same calendar period
        var expiredFunds = (await expiredFundsRepository.Get(request.AccountId)).ToList();

        return new GetExpiredFundsByAccountIdQueryResult(
            expiredFunds.Where(fund => fund.TransactionType == LongTermTransactionType).ToCalendarPeriodDictionary(),
            expiredFunds.Where(fund => fund.TransactionType == ShortTermTransactionType).ToCalendarPeriodDictionary());
    }
}
