using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;

public sealed record GetLevySummaryByAccountIdQuery(long AccountId)
    : IRequest<GetLevySummaryByAccountIdQueryResult>;