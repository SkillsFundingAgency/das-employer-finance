namespace SFA.DAS.EmployerFinance.Queries.GetLatestTransactionTypeTransactionIdsForAccount;

public sealed record GetLatestTransactionTypeTransactionIdsForAccountQueryResult(
    long? LatestDeclarationId,
    long? LatestPaymentId,
    long? LatestExpiryId);
