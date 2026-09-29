namespace SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummaryByDate;

public sealed record GetAccountTransactionSummaryByDateQuery(long AccountId, DateTime FromDate, DateTime ToDate)
    : IRequest<GetAccountTransactionSummaryByDateQueryResult>;