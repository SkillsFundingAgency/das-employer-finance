using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;

public class FindAccountProviderPaymentsHandler(
    IValidator<FindAccountProviderPaymentsQuery> validator,
    IDasLevyService dasLevyService,
    IEncodingService encodingService)
    : IRequestHandler<FindAccountProviderPaymentsQuery, FindAccountProviderPaymentsResponse>
{
    public async Task<FindAccountProviderPaymentsResponse> Handle(FindAccountProviderPaymentsQuery message,CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        if (validationResult.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        var accountId = encodingService.Decode(message.HashedAccountId,EncodingType.AccountId);
        var transactions = await dasLevyService.GetAccountProviderPaymentsByDateRange<PaymentTransactionLine>
            (accountId, message.UkPrn, message.FromDate, message.ToDate);

        if (!transactions.Any())
        {
            return null;//TODO
        }

        var firstTransaction = transactions.First();

        return new FindAccountProviderPaymentsResponse
        {
            ProviderName = firstTransaction.ProviderName,
            TransactionDate = firstTransaction.TransactionDate,
            DateCreated = firstTransaction.DateCreated,
            Transactions = transactions.ToList(),
            Total = transactions.Sum(c => c.LineAmount)
        };
    }
}