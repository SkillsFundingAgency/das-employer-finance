using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;

public class GetTransferTransactionDetailsQueryHandler : IRequestHandler<GetTransferTransactionDetailsQuery, GetTransferTransactionDetailsResponse>
{
    private readonly Lazy<EmployerFinanceDbContext> _dbContext;
    private readonly IEncodingService _encodingService;

    public GetTransferTransactionDetailsQueryHandler(Lazy<EmployerFinanceDbContext> dbContext,
        IEncodingService encodingService)
    {
        _dbContext = dbContext;
        _encodingService = encodingService;
    }

    public async Task<GetTransferTransactionDetailsResponse> Handle(GetTransferTransactionDetailsQuery query, CancellationToken cancellationToken)
    {
        var targetAccountId = _encodingService.Decode(query.TargetAccountPublicHashedId, EncodingType.PublicAccountId);

        var transfers = await _dbContext.Value.AccountTransfers
            .Where(x =>
                        (x.SenderAccountId == query.AccountId.GetValueOrDefault() && x.ReceiverAccountId == targetAccountId)
                        ||
                        (x.SenderAccountId == targetAccountId && x.ReceiverAccountId == query.AccountId.GetValueOrDefault())
                        && x.PeriodEnd == query.PeriodEnd)
            .ToListAsync(cancellationToken);

        var firstTransfer = transfers.First();

        var senderAccountName = firstTransfer.SenderAccountName;
        var senderPublicHashedAccountId = _encodingService.Encode(firstTransfer.SenderAccountId, EncodingType.PublicAccountId);

        var receiverAccountName = firstTransfer.ReceiverAccountName;
        var receiverPublicHashedAccountId = _encodingService.Encode(firstTransfer.ReceiverAccountId, EncodingType.PublicAccountId);

        var courseTransfers = transfers.GroupBy(t => new { t.CourseName, t.CourseLevel });

        var transferDetails = courseTransfers.Select(ct => new AccountTransferDetails
        {
            CourseName = ct.First().CourseName,
            CourseLevel = ct.First().CourseLevel,
            PaymentTotal = ct.Sum(t => t.Amount),
            ApprenticeCount = (uint)ct.DistinctBy(t => t.ApprenticeshipId).Count()
        }).ToArray();

        //NOTE: We should only get one transfer transaction per sender per period end
        // as this is how transfers are grouped together when creating transfer transactions
        var transferTransaction = _dbContext.Value.Transactions.Single(lineEntity =>
            lineEntity.AccountId == query.AccountId &&
            lineEntity.TransactionType == TransactionItemType.Transfer &&
            lineEntity.TransferSenderAccountId != null &&
            lineEntity.TransferReceiverAccountId != null &&
            lineEntity.TransferSenderAccountId == firstTransfer.SenderAccountId &&
            lineEntity.TransferReceiverAccountId == firstTransfer.ReceiverAccountId &&
            lineEntity.PeriodEnd.Equals(query.PeriodEnd));

        var transferDate = transferTransaction.DateCreated;
        var transfersPaymentTotal = transferDetails.Sum(t => t.PaymentTotal);

        var isCurrentAccountSender = query.AccountId.GetValueOrDefault() == firstTransfer.SenderAccountId;

        return new GetTransferTransactionDetailsResponse
        {
            SenderAccountName = senderAccountName,
            SenderAccountPublicHashedId = senderPublicHashedAccountId,
            ReceiverAccountName = receiverAccountName,
            ReceiverAccountPublicHashedId = receiverPublicHashedAccountId,
            IsCurrentAccountSender = isCurrentAccountSender,
            TransferDetails = transferDetails,
            TransferPaymentTotal = transfersPaymentTotal,
            DateCreated = transferDate
        };
    }
}