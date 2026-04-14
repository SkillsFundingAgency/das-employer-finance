using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.GetExistingTransactionLines;

public class GetExistingTransactionLinesHandler : IRequestHandler<GetExistingTransactionLinesQuery, GetEmployerAccountTransactionsResponse>
{
    private readonly IDasLevyService _dasLevyService;
    private readonly IValidator<GetExistingTransactionLinesQuery> _validator;
    private readonly IEncodingService _encodingService;
    private readonly ILogger<GetExistingTransactionLinesHandler> _logger;

    public GetExistingTransactionLinesHandler(
        IDasLevyService dasLevyService,
        IValidator<GetExistingTransactionLinesQuery> validator,
        ILogger<GetExistingTransactionLinesHandler> logger,
        IEncodingService encodingService)
    {
        _dasLevyService = dasLevyService;
        _validator = validator;
        _logger = logger;
        _encodingService = encodingService;
    }

    public async Task<GetEmployerAccountTransactionsResponse> Handle(GetExistingTransactionLinesQuery message, CancellationToken cancellationToken)
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

        var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var transactions = await _dasLevyService.GetExistingTransactionLines(accountId, message.PeriodEnd, message.TransactionType);
        var balance = await _dasLevyService.GetAccountBalance(accountId);

        foreach (var transaction in transactions)
        {
            await GenerateTransactionDescription(transaction);
        }

        PopulateTransferPublicHashedIds(transactions);

        return new GetEmployerAccountTransactionsResponse
        {
            Data = new AggregationData
            {
                HashedAccountId = message.HashedAccountId,
                AccountId = accountId,
                Balance = balance,
                TransactionLines = transactions
            },
            AccountHasPreviousTransactions = false
        };
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
            _logger.LogInformation($"Provider not found for UkPrn:{transaction.UkPrn} - {ex.Message}");
        }

        return $"{transactionPrefix}Training provider - name not recognised";
    }

    private void PopulateTransferPublicHashedIds(IEnumerable<TransactionLine> transactions)
    {
        var transferTransactions = transactions.OfType<TransferTransactionLine>();

        foreach (var transaction in transferTransactions)
        {
            transaction.ReceiverAccountPublicHashedId =
                _encodingService.Encode(transaction.ReceiverAccountId, EncodingType.PublicAccountId);

            transaction.SenderAccountPublicHashedId =
                _encodingService.Encode(transaction.SenderAccountId, EncodingType.PublicAccountId);
        }
    }
}
