using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

public class GetEmployerAccountTransactionsHandler(
    IDasLevyService dasLevyService,
    IValidator<GetEmployerAccountTransactionsQuery> validator,
    ILogger<GetEmployerAccountTransactionsHandler> logger,
    IEncodingService encodingService)
    :
        IRequestHandler<GetEmployerAccountTransactionsQuery, GetEmployerAccountTransactionsResponse>
{
    public async Task<GetEmployerAccountTransactionsResponse> Handle(GetEmployerAccountTransactionsQuery message, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(message);

        if (!result.IsValid())
        {
            throw new ValidationException(result.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        if (result.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        var accountId = encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var transactions = await dasLevyService.GetAccountTransactionsByDateRange(accountId, message.FromDate, message.ToDate);
        var balance = await dasLevyService.GetAccountBalance(accountId);

        var hasPreviousTransactions = await dasLevyService.GetPreviousAccountTransaction(accountId, message.FromDate) > 0;

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
            message.ToDate.Year,
            message.ToDate.Month);
    }

    private async Task GenerateTransactionDescription(TransactionLine transaction)
    {
        if (transaction.GetType() == typeof(LevyDeclarationTransactionLine))
        {
            transaction.Description = transaction.Amount >= 0 ? "Levy declared this month" : "Levy adjustment";
        }
        else if (transaction.GetType() == typeof(PaymentTransactionLine))
        {
            var paymentTransaction = (PaymentTransactionLine)transaction;

            transaction.Description = await GetPaymentTransactionDescription(paymentTransaction);
        }
        else if (transaction.GetType() == typeof(ExpiredFundTransactionLine))
        {
            transaction.Description = "Expired levy this month";
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
                transaction.TransferSourceDescription = $"Paid using transfer from {transferTransaction.SenderAccountName}";
            }
        }
    }

    private async Task<string> GetPaymentTransactionDescription(PaymentTransactionLine transaction)
    {
        var transactionPrefix = transaction.IsCoInvested ? "Co-investment - " : string.Empty;

        try
        {
            var ukprn = Convert.ToInt32(transaction.UkPrn);
            var providerName = await dasLevyService.GetProviderName(ukprn, transaction.AccountId, transaction.PeriodEnd);
            if (providerName != null)
                return $"{transactionPrefix}{providerName}";
        }
        catch (Exception ex)
        {
            logger.LogInformation("Provider not found for UkPrn:{TransactionUkPrn} - {ExMessage}", transaction.UkPrn, ex.Message);
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
                encodingService.Encode(transaction.ReceiverAccountId, EncodingType.PublicAccountId);

            transaction.SenderAccountPublicHashedId =
                encodingService.Encode(transaction.SenderAccountId, EncodingType.PublicAccountId);
        }
    }
}