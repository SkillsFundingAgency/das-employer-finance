using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyFundsInByAccountId;

public class GetLevyFundsInByAccountIdQueryHandler(ILevyFundsInRepository levyFundsInRepository)
    : IRequestHandler<GetLevyFundsInByAccountIdQuery, GetLevyFundsInByAccountIdQueryResult>
{
    public async Task<GetLevyFundsInByAccountIdQueryResult> Handle(GetLevyFundsInByAccountIdQuery request, CancellationToken cancellationToken)
    {
        var fundsIn = await levyFundsInRepository.GetLevyFundsIn(request.AccountId);
        return new GetLevyFundsInByAccountIdQueryResult(fundsIn.ToCalendarPeriodDictionary());
    }
}
