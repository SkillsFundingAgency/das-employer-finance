using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;

public class GetTransferTransactionDetailsQueryHandler(
    Lazy<EmployerFinanceDbContext> dbContext,
    IEncodingService encodingService,
    ILogger<GetTransferTransactionDetailsQueryHandler> logger)
    : IRequestHandler<GetTransferTransactionDetailsQuery,
        GetTransferTransactionDetailsResponse>
{
    public async Task<GetTransferTransactionDetailsResponse> Handle(GetTransferTransactionDetailsQuery query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("{TypeName} processing started.", nameof(GetTransferTransactionDetailsQueryHandler));

        var targetAccountId = encodingService.Decode(query.TargetAccountPublicHashedId, EncodingType.PublicAccountId);

        var transfers = await (from at in dbContext.Value.AccountTransfers
            join p in dbContext.Value.Payments on 
                new { AccountId = at.ReceiverAccountId, at.PeriodEnd, at.ApprenticeshipId } equals 
                new { AccountId = p.EmployerAccountId, PeriodEnd = p.CollectionPeriodId, p.ApprenticeshipId }
            join pmd in dbContext.Value.PaymentMetaData on p.PaymentMetaDataId equals pmd.Id
            where ((at.SenderAccountId == query.AccountId.GetValueOrDefault() &&
                    at.ReceiverAccountId == targetAccountId)
                   || (at.SenderAccountId == targetAccountId &&
                       at.ReceiverAccountId == query.AccountId.GetValueOrDefault()))
                  && at.PeriodEnd == query.PeriodEnd
            select new
            {
                at.SenderAccountId,
                at.SenderAccountName,
                at.ReceiverAccountId,
                at.ReceiverAccountName,
                p.Amount,
                at.ApprenticeshipId,
                CourseName = pmd.ApprenticeshipCourseName,
                CourseLevel = pmd.ApprenticeshipCourseLevel,
                CohortId = pmd.CohortId
            }).ToListAsync(cancellationToken);

        var firstTransfer = transfers.First();

        var senderAccountName = firstTransfer.SenderAccountName;
        var senderPublicHashedAccountId =
            encodingService.Encode(firstTransfer.SenderAccountId, EncodingType.PublicAccountId);

        var receiverAccountName = firstTransfer.ReceiverAccountName;
        var receiverPublicHashedAccountId =
            encodingService.Encode(firstTransfer.ReceiverAccountId, EncodingType.PublicAccountId);

        // Grouping by CourseName and CourseLevel (from PaymentMetadata)
        var courseTransfers = transfers.GroupBy(accountTransfer =>
            new { accountTransfer.CourseName, accountTransfer.CourseLevel });

        var transferDetails = courseTransfers.Select(courseTransfer => new AccountTransferDetails
        {
            CourseName = courseTransfer.Key.CourseName,
            CourseLevel = courseTransfer.Key.CourseLevel,
            PaymentTotal = courseTransfer.Sum(t => t.Amount),
            ApprenticeCount = (uint)courseTransfer.DistinctBy(t => t.ApprenticeshipId).Count(),
            CohortReference = EncodeCohortReference(courseTransfer.First()?.CohortId)
        }).ToArray();

        // Ensure single transfer transaction is retrieved
        var transferTransaction = dbContext.Value.Transactions.Single(transaction =>
            transaction.AccountId == query.AccountId &&
            transaction.TransactionType == TransactionItemType.Transfer &&
            transaction.TransferSenderAccountId != null &&
            transaction.TransferReceiverAccountId != null &&
            transaction.TransferSenderAccountId == firstTransfer.SenderAccountId &&
            transaction.TransferReceiverAccountId == firstTransfer.ReceiverAccountId &&
            transaction.PeriodEnd.Equals(query.PeriodEnd));

        var transferDate = transferTransaction.DateCreated;
        var transfersPaymentTotal = transferDetails.Sum(details => details.PaymentTotal);

        var isCurrentAccountSender = query.AccountId.GetValueOrDefault() == firstTransfer.SenderAccountId;

        logger.LogInformation("{TypeName} processing competed.", nameof(GetTransferTransactionDetailsQueryHandler));

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

    private string EncodeCohortReference(long? cohortId)
    {
        return !cohortId.HasValue
            ? null
            : encodingService.Encode(cohortId.Value, EncodingType.CohortReference);
    }
}