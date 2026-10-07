using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummaryByDate;

public class GetAccountTransactionSummaryByDateQueryHandler(ITransactionRepository transactionRepository, IValidator<GetAccountTransactionSummaryByDateQuery> validator) 
    : IRequestHandler<GetAccountTransactionSummaryByDateQuery, GetAccountTransactionSummaryByDateQueryResult>
{
    public async Task<GetAccountTransactionSummaryByDateQueryResult> Handle(GetAccountTransactionSummaryByDateQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var result = await transactionRepository.GetAccountTransactionsByDateRange(request.AccountId, request.FromDate, request.ToDate);
        return new GetAccountTransactionSummaryByDateQueryResult(result);
    }
}