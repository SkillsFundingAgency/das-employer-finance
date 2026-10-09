namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndDateRange;

public sealed record GetLevyDeclarationsByAccountAndDateRangeQuery(long AccountId, DateOnly FromDate, DateOnly ToDate)
    : IRequest<GetLevyDeclarationsByAccountAndDateRangeQueryResult>;