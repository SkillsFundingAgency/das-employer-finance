using System.Text.Json;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;

public class GetTransferTransactionDetailsQueryHandler : IRequestHandler<GetTransferTransactionDetailsQuery, GetTransferTransactionDetailsResponse>
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

    public async Task<GetTransferTransactionDetailsResponse> Handle(GetTransferTransactionDetailsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{TypeName} processing started.", nameof(GetTransferTransactionDetailsQueryHandler));
        
        _logger.LogInformation("{TypeName} query details: {Query}", nameof(GetTransferTransactionDetailsQueryHandler), JsonSerializer.Serialize(query));
        
        var targetAccountId = _encodingService.Decode(query.TargetAccountPublicHashedId, EncodingType.PublicAccountId);
        
        _logger.LogInformation("{TypeName} target accountId : {TargetAccountId}", nameof(GetTransferTransactionDetailsQueryHandler), targetAccountId);

        var transfers = await _dbContext.Value.AccountTransfers
            .Where(accountTransfer =>
                        (accountTransfer.SenderAccountId == query.AccountId.GetValueOrDefault() && accountTransfer.ReceiverAccountId == targetAccountId)
                        ||
                        (accountTransfer.SenderAccountId == targetAccountId && accountTransfer.ReceiverAccountId == query.AccountId.GetValueOrDefault())
                        && accountTransfer.PeriodEnd == query.PeriodEnd)
            .ToListAsync(cancellationToken);

        var firstTransfer = transfers.First();

        var senderAccountName = firstTransfer.SenderAccountName;
        var senderPublicHashedAccountId = _encodingService.Encode(firstTransfer.SenderAccountId, EncodingType.PublicAccountId);

        var receiverAccountName = firstTransfer.ReceiverAccountName;
        var receiverPublicHashedAccountId = _encodingService.Encode(firstTransfer.ReceiverAccountId, EncodingType.PublicAccountId);

        var courseTransfers = transfers.GroupBy(accountTransfer => new { accountTransfer.CourseName, accountTransfer.CourseLevel });

        var transferDetails = courseTransfers.Select(courseTransfer => new AccountTransferDetails
        {
            CourseName = courseTransfer.First().CourseName,
            CourseLevel = courseTransfer.First().CourseLevel,
            PaymentTotal = courseTransfer.Sum(t => t.Amount),
            ApprenticeCount = (uint)courseTransfer.DistinctBy(t => t.ApprenticeshipId).Count()
        }).ToArray();

        //NOTE: We should only get one transfer transaction per sender per period end
        // as this is how transfers are grouped together when creating transfer transactions
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