﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.MarkerInterfaces;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions
{
    public class GetEmployerAccountTransactionsHandler :
        IRequestHandler<GetEmployerAccountTransactionsQuery, GetEmployerAccountTransactionsResponse>
    {
        private readonly IDasLevyService _dasLevyService;
        private readonly IValidator<GetEmployerAccountTransactionsQuery> _validator;
        private readonly IEncodingService _encodingService;
        private readonly IPublicHashingService _publicHashingService;
        private readonly ILog _logger;

        public GetEmployerAccountTransactionsHandler(
            IDasLevyService dasLevyService,
            IValidator<GetEmployerAccountTransactionsQuery> validator,
            ILog logger,
            IEncodingService encodingService,
            IPublicHashingService publicHashingService)
        {
            _dasLevyService = dasLevyService;
            _validator = validator;
            _logger = logger;
            _encodingService = encodingService;
            _publicHashingService = publicHashingService;
        }

        public async Task<GetEmployerAccountTransactionsResponse> Handle(GetEmployerAccountTransactionsQuery message,CancellationToken  cancellationToken)
        {
            var result = await _validator.ValidateAsync(message);

            if (!result.IsValid())
            {
                throw new ValidationException(result.ConvertToDataAnnotationsValidationResult(), null, null);
            }            

            if (result.IsUnauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            var toDate = CalculateToDate(message);
            var fromDate = new DateTime(toDate.Year, toDate.Month, 1);

            var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
            var transactions = await _dasLevyService.GetAccountTransactionsByDateRange(accountId, fromDate, toDate);
            var balance = await _dasLevyService.GetAccountBalance(accountId);

            var hasPreviousTransactions = await _dasLevyService.GetPreviousAccountTransaction(accountId, fromDate) > 0;

            foreach (var transaction in transactions)
            {
                await GenerateTransactionDescription(transaction);
            }

            PopulateTransferPublicHashedIds(transactions);

            return GetResponse(
                message.HashedAccountId, 
                accountId, 
                transactions, 
                balance, 
                hasPreviousTransactions, 
                toDate.Year, 
                toDate.Month);
        }

        private static DateTime CalculateToDate(GetEmployerAccountTransactionsQuery message)
        {
            var year = message.Year == default(int) ? DateTime.Now.Year : message.Year;
            var month = message.Month == default(int) ? DateTime.Now.Month : message.Month;

            var daysInMonth = DateTime.DaysInMonth(year, month);

            var toDate = new DateTime(year, month, daysInMonth);
            return toDate;
        }

        private async Task GenerateTransactionDescription(TransactionLine transaction)
        {
            if (transaction.GetType() == typeof(LevyDeclarationTransactionLine))
            {
                transaction.Description = transaction.Amount >= 0 ? "Levy" : "Levy adjustment";
            }
            else if (transaction.GetType() == typeof(PaymentTransactionLine))
            {
                var paymentTransaction = (PaymentTransactionLine)transaction;

                transaction.Description = await GetPaymentTransactionDescription(paymentTransaction);
            }
            else if (transaction.GetType() == typeof(ExpiredFundTransactionLine))
            {
                transaction.Description = "Expired levy";
            }
            else if (transaction.GetType() == typeof(TransferTransactionLine))
            {
                var transferTransaction = (TransferTransactionLine)transaction;

                if (transferTransaction.TransactionAccountIsTransferSender)
                {
                    transaction.Description = $"Transfer sent to {transferTransaction.ReceiverAccountName}";
                }
                else
                {
                    transaction.Description = $"Transfer received from {transferTransaction.SenderAccountName}";
                }
            }
        }

        private async Task<string> GetPaymentTransactionDescription(PaymentTransactionLine transaction)
        {
            var transactionPrefix = transaction.IsCoInvested ? "Co-investment - " : string.Empty;

            try
            {
                var ukprn = Convert.ToInt32(transaction.UkPrn);
                var providerName = await _dasLevyService.GetProviderName(ukprn, transaction.AccountId, transaction.PeriodEnd);
                if (providerName != null)
                    return $"{transactionPrefix}{providerName}";
            }
            catch (Exception ex)
            {
                _logger.Info($"Provider not found for UkPrn:{transaction.UkPrn} - {ex.Message}");
            }

            return $"{transactionPrefix}Training provider - name not recognised";
        }

        private static GetEmployerAccountTransactionsResponse GetResponse(
            string hashedAccountId, 
            long accountId, 
            TransactionLine[] transactions, 
            decimal balance,
            bool hasPreviousTransactions, 
            int year, 
            int month)
        {
            return new GetEmployerAccountTransactionsResponse
            {
                Data = new AggregationData
                {
                    HashedAccountId = hashedAccountId,
                    AccountId = accountId,
                    Balance = balance,
                    TransactionLines = transactions
                },
                AccountHasPreviousTransactions = hasPreviousTransactions,
                Year = year,
                Month = month
            };
        }

        private void PopulateTransferPublicHashedIds(IEnumerable<TransactionLine> transactions)
        {
            var transferTransactions = transactions.OfType<TransferTransactionLine>();

            foreach (var transaction in transferTransactions)
            {
                transaction.ReceiverAccountPublicHashedId =
                    _publicHashingService.HashValue(transaction.ReceiverAccountId);

                transaction.SenderAccountPublicHashedId =
                    _publicHashingService.HashValue(transaction.SenderAccountId);
            }
        }
    }
}