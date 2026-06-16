using System.ComponentModel.DataAnnotations;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.Queries.GetTransactionsDownload;

public class GetTransactionsDownloadQueryHandler(
    ITransactionFormatterFactory transactionsFormatterFactory,
    ITransactionRepository transactionRepository,
    IAccountApiClient accountApiClient,
    IEncodingService encodingService)
    : IRequestHandler<GetTransactionsDownloadQuery, GetTransactionsDownloadResponse>
{
    public async Task<GetTransactionsDownloadResponse> Handle(GetTransactionsDownloadQuery message,CancellationToken cancellationToken)
    {
        var endDate = message.EndDate.ToDate();
        var endDateBeginningOfNextMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1);
        var transactions = await transactionRepository.GetAllTransactionDetailsForAccountByDate(message.AccountId, message.StartDate, endDateBeginningOfNextMonth);
                       
        if (!transactions.Any())
        {
            var validationResult = new ValidationResult{ValidationDictionary = new Dictionary<string, string>{{"StartDate","There are no transactions in the date range"}}};
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var accountResponse = await accountApiClient.GetAccount(message.AccountId);
        var apprenticeshipEmployerTypeEnum = (ApprenticeshipEmployerType)Enum.Parse(typeof(ApprenticeshipEmployerType), accountResponse.ApprenticeshipEmployerType, true);

        foreach (var transaction in transactions)
        {
            GenerateTransactionDescription(transaction);
            EncodeCohortReference(transaction);
        }

        var fileFormatter = transactionsFormatterFactory.GetTransactionsFormatterByType(
            message.DownloadFormat.Value,
            apprenticeshipEmployerTypeEnum);

        return new GetTransactionsDownloadResponse
        {
            FileData = fileFormatter.GetFileData(transactions, message.Version == "2"),
            FileExtension = fileFormatter.FileExtension,
            MimeType = fileFormatter.MimeType
        };
    }

    private void EncodeCohortReference(TransactionDownloadLine transaction)
    {
        if(!transaction.CohortId.HasValue)
        {
            return;
        }

        transaction.CohortReference = encodingService.Encode(transaction.CohortId.Value, EncodingType.CohortReference);
    }

    private static void GenerateTransactionDescription(TransactionDownloadLine transaction)
    {
        transaction.Description = transaction.TransactionType;

        if (transaction.TransactionType.Equals("Payment", StringComparison.OrdinalIgnoreCase))
        {
            transaction.Description = transaction.TrainingProviderFormatted;
        }
        else if (transaction.TransactionType.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
        {
            transaction.Description = transaction.TransferSenderAccountId.Equals(transaction.AccountId) 
                ? $"Transfer sent to {transaction.TransferReceiverAccountName}" 
                : $"Transfer received from {transaction.TransferSenderAccountName}";
        }
    }
}