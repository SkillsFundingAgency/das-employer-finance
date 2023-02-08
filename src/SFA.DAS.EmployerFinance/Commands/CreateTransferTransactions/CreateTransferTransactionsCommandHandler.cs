using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.NLog.Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions
{
    public class CreateTransferTransactionsCommandHandler : IRequestHandler<CreateTransferTransactionsCommand,Unit>
    {
        private readonly IValidator<CreateTransferTransactionsCommand> _validator;
        private readonly ITransferRepository _transferRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILog _logger;


        public CreateTransferTransactionsCommandHandler(
            IValidator<CreateTransferTransactionsCommand> validator,
            ITransferRepository transferRepository,
            ITransactionRepository transactionRepository,
            ILog logger)
        {
            _validator = validator;
            _transferRepository = transferRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreateTransferTransactionsCommand request,CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid())
            {
                throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            }

            try
            {
                var transfers = await _transferRepository.GetReceiverAccountTransfersByPeriodEnd(request.ReceiverAccountId, request.PeriodEnd);

                var accountTransfers = transfers as AccountTransfer[] ?? transfers.ToArray();

                //If this code changes you need to check that the transfer transaction details 
                // code supports the change as it currently relies on grouping by sender for the transactions
                var senderTransfers = accountTransfers.GroupBy(t => t.SenderAccountId);

                var transactions = senderTransfers.SelectMany(CreateTransactions).ToArray();

                await _transactionRepository.CreateTransferTransactions(transactions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to create transfer transaction for accountId {request.ReceiverAccountId} and period end {request.PeriodEnd}");
                throw;
            }

            return Unit.Value;
        }

        private static IEnumerable<TransferTransactionLine> CreateTransactions(IGrouping<long, AccountTransfer> senderTransferGroup)
        {
            if (!senderTransferGroup.Any())
                return new TransferTransactionLine[0];

            var firstTransfer = senderTransferGroup.First();

            var senderAccountId = senderTransferGroup.Key;
            var senderAccountName = firstTransfer.SenderAccountName;
            var receiverAccountId = firstTransfer.ReceiverAccountId;  //use key as we have grouped by receiver ID
            var receiverAccountName = firstTransfer.ReceiverAccountName;
            var transferTotal = senderTransferGroup.Sum(gt => gt.Amount);
            var periodEnd = firstTransfer.PeriodEnd;

            var senderTransferTransaction = new TransferTransactionLine
            {
                AccountId = senderAccountId,
                Amount = -transferTotal,        //Negative value as we are removing money from sender
                DateCreated = DateTime.Now,
                TransactionDate = DateTime.Now,
                SenderAccountId = senderAccountId,
                SenderAccountName = senderAccountName,
                ReceiverAccountId = receiverAccountId,
                ReceiverAccountName = receiverAccountName,
                PeriodEnd = periodEnd
            };

            var receiverTransferTransaction = new TransferTransactionLine
            {
                AccountId = receiverAccountId,
                Amount = transferTotal,         //Positive value as we are adding money to receiver
                DateCreated = DateTime.Now,
                TransactionDate = DateTime.Now,
                SenderAccountId = senderAccountId,
                SenderAccountName = senderAccountName,
                ReceiverAccountId = receiverAccountId,
                ReceiverAccountName = receiverAccountName,
                PeriodEnd = periodEnd
            };

            return new[] { senderTransferTransaction, receiverTransferTransaction };
        }
    }
}