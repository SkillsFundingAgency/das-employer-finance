using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.MarkerInterfaces;
//using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails
{
    public class GetTransferTransactionDetailsQueryHandler : IRequestHandler<GetTransferTransactionDetailsQuery, GetTransferTransactionDetailsResponse>
    {
        private readonly Lazy<EmployerFinanceDbContext> _dbContext;
        private readonly IPublicHashingService _publicHashingService;
        private readonly IEncodingService _encodingService;

        public GetTransferTransactionDetailsQueryHandler(Lazy<EmployerFinanceDbContext> dbContext,
            IEncodingService encodingService)
        {
            _dbContext = dbContext;
            _encodingService = encodingService;
        }

        public async Task<GetTransferTransactionDetailsResponse> Handle(GetTransferTransactionDetailsQuery query,CancellationToken cancellationToken)
        {
            var targetAccountId = _encodingService.Decode(query.TargetAccountPublicHashedId, EncodingType.PublicAccountId);

            var result = await new Lazy<EmployerFinanceDbContext>(() => _dbContext.Value).GetTransfersByTargetAccountId(
                                    query.AccountId.GetValueOrDefault(),
                                    targetAccountId,
                                    query.PeriodEnd);

            var transfers = result as List<AccountTransfer> ?? result.ToList();

            var firstTransfer = transfers.First();

            var senderAccountName = firstTransfer.SenderAccountName;
            var senderPublicHashedAccountId = _publicHashingService.HashValue(firstTransfer.SenderAccountId);

            var receiverAccountName = firstTransfer.ReceiverAccountName;
            var receiverPublicHashedAccountId = _publicHashingService.HashValue(firstTransfer.ReceiverAccountId);

            var courseTransfers = transfers.GroupBy(t => new { t.CourseName, t.CourseLevel });

            var x = courseTransfers.ToArray();

            var transferDetails = courseTransfers.Select(ct => new AccountTransferDetails
            {
                CourseName = ct.First().CourseName,
                CourseLevel = ct.First().CourseLevel,
                PaymentTotal = ct.Sum(t => t.Amount),
                ApprenticeCount = (uint)ct.DistinctBy(t => t.ApprenticeshipId).Count()
            }).ToArray();

            //NOTE: We should only get one transfer transaction per sender per period end
            // as this is how transfers are grouped together when creating transfer transactions
            var transferTransaction = _dbContext.Value.Transactions.Single(t =>
                t.AccountId == query.AccountId &&
                t.TransactionType == TransactionItemType.Transfer &&
                t.TransferSenderAccountId != null &&
                t.TransferReceiverAccountId != null &&
                t.TransferSenderAccountId == firstTransfer.SenderAccountId &&
                t.TransferReceiverAccountId == firstTransfer.ReceiverAccountId &&
                t.PeriodEnd.Equals(query.PeriodEnd));

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
}