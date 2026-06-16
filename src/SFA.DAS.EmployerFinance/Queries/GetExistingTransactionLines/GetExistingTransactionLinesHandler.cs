using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EmployerFinance.Queries.GetExistingTransactionLines;

public class GetExistingTransactionLinesHandler(IDasLevyService dasLevyService, IValidator<GetExistingTransactionLinesQuery> validator,
                                                 ILogger<GetExistingTransactionLinesHandler> logger, IEncodingService encodingService) :
                                                 IRequestHandler<GetExistingTransactionLinesQuery, GetEmployerAccountTransactionsResponse>
{
    public async Task<GetEmployerAccountTransactionsResponse> Handle(GetExistingTransactionLinesQuery message, CancellationToken cancellationToken)
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
        var transactions = await dasLevyService.GetExistingTransactionLines(accountId, message.PeriodEnd, message.TransactionType);

        return new GetEmployerAccountTransactionsResponse
        {
            Data = new AggregationData
            {
                HashedAccountId = message.HashedAccountId,
                AccountId = accountId,
                TransactionLines = transactions
            },
            AccountHasPreviousTransactions = false
        };
    }
}