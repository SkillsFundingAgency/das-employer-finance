namespace SFA.DAS.EmployerFinance.Api.Types;

public sealed record LatestTransactionTypeTransactionIdsResponse
{
    public long? LatestDeclarationId { get; init; }

    public long? LatestPaymentId { get; init; }

    public long? LatestExpiryId { get; init; }
}
