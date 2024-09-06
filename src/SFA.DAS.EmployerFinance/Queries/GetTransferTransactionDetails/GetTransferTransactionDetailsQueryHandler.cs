using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;

public class GetTransferTransactionDetailsQueryHandler : IRequestHandler<GetTransferTransactionDetailsQuery,
    GetTransferTransactionDetailsResponse>
{
    private readonly Lazy<EmployerFinanceDbContext> _dbContext;
    private readonly IEncodingService _encodingService;
    private readonly ILogger<GetTransferTransactionDetailsQueryHandler> _logger;

    public GetTransferTransactionDetailsQueryHandler(Lazy<EmployerFinanceDbContext> dbContext,
        IEncodingService encodingService,
        ILogger<GetTransferTransactionDetailsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _encodingService = encodingService;
        _logger = logger;
    }

    public async Task<GetTransferTransactionDetailsResponse> Handle(GetTransferTransactionDetailsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("{TypeName} processing started.", nameof(GetTransferTransactionDetailsQueryHandler));

        var targetAccountId = _encodingService.Decode(query.TargetAccountPublicHashedId, EncodingType.PublicAccountId);

        var transfers = await (from at in _dbContext.Value.AccountTransfers
            join p in _dbContext.Value.Payments on 
                new { AccountId = at.ReceiverAccountId, at.PeriodEnd, at.ApprenticeshipId } equals 
                new { AccountId = p.EmployerAccountId, PeriodEnd = p.CollectionPeriodId, p.ApprenticeshipId }
            join pmd in _dbContext.Value.PaymentMetaData on p.PaymentMetaDataId equals pmd.Id
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
                at.Amount,
                at.ApprenticeshipId,
                CourseName = pmd.ApprenticeshipCourseName,
                CourseLevel = pmd.ApprenticeshipCourseLevel
            }).ToListAsync(cancellationToken);

        var firstTransfer = transfers.First();

        var senderAccountName = firstTransfer.SenderAccountName;
        var senderPublicHashedAccountId =
            _encodingService.Encode(firstTransfer.SenderAccountId, EncodingType.PublicAccountId);

        var receiverAccountName = firstTransfer.ReceiverAccountName;
        var receiverPublicHashedAccountId =
            _encodingService.Encode(firstTransfer.ReceiverAccountId, EncodingType.PublicAccountId);

        // Grouping by CourseName and CourseLevel (from PaymentMetadata)
        var courseTransfers = transfers.GroupBy(accountTransfer =>
            new { accountTransfer.CourseName, accountTransfer.CourseLevel });

        var transferDetails = courseTransfers.Select(courseTransfer => new AccountTransferDetails
        {
            CourseName = courseTransfer.Key.CourseName,
            CourseLevel = courseTransfer.Key.CourseLevel,
            PaymentTotal = courseTransfer.Sum(t => t.Amount),
            ApprenticeCount = (uint)courseTransfer.DistinctBy(t => t.ApprenticeshipId).Count()
        }).ToArray();

        // Ensure single transfer transaction is retrieved
        var transferTransaction = _dbContext.Value.Transactions.Single(transaction =>
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

        _logger.LogInformation("{TypeName} processing competed.", nameof(GetTransferTransactionDetailsQueryHandler));

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