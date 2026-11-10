using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Queries.GetLatestTransactionTypeTransactionIdsForAccount;

public class GetLatestTransactionTypeTransactionIdsForAccountQueryHandler(ITransactionRepository transactionRepository)
    : IRequestHandler<GetLatestTransactionTypeTransactionIdsForAccountQuery, GetLatestTransactionTypeTransactionIdsForAccountQueryResult>
{
    private static readonly TransactionItemType[] DeclarationTypes = [TransactionItemType.Declaration, TransactionItemType.TopUp];
    private static readonly TransactionItemType[] PaymentTypes = [TransactionItemType.Payment, TransactionItemType.Transfer];
    private static readonly TransactionItemType[] ExpiryTypes = [TransactionItemType.ExpiredFund, TransactionItemType.ShortExpiredFund];

    public async Task<GetLatestTransactionTypeTransactionIdsForAccountQueryResult> Handle(GetLatestTransactionTypeTransactionIdsForAccountQuery request, CancellationToken cancellationToken)
    {
        var latestIds = (await transactionRepository.GetLatestTransactionTypeTransactionIdsForAccount(request.AccountId)).ToList();

        return new GetLatestTransactionTypeTransactionIdsForAccountQueryResult(
            LatestFor(latestIds, DeclarationTypes),
            LatestFor(latestIds, PaymentTypes),
            LatestFor(latestIds, ExpiryTypes));
    }

    private static long? LatestFor(IEnumerable<LatestTransactionTypeTransactionId> latestIds, TransactionItemType[] transactionTypes) =>
        latestIds
            .Where(l => transactionTypes.Contains(l.TransactionType))
            .Max(l => (long?)l.LatestId);
}
