using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetCollectionPeriodPaymentsByAccountId;

public sealed record GetCollectionPeriodPaymentsByAccountIdQueryResult(
    bool PeriodEndCompleted,
    IReadOnlyCollection<CollectionPeriodPaymentTotal> Payments);

public sealed record CollectionPeriodPaymentTotal(FundingSource FundingSource, TransactionType TransactionType, decimal Amount);
