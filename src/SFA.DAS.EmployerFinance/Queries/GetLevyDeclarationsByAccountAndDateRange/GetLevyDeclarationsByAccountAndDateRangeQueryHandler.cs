using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndDateRange;

public class GetLevyDeclarationsByAccountAndDateRangeQueryHandler(IDasLevyRepository dasLevyRepository,
    IValidator<GetLevyDeclarationsByAccountAndDateRangeQuery> validator) 
    : IRequestHandler<GetLevyDeclarationsByAccountAndDateRangeQuery, GetLevyDeclarationsByAccountAndDateRangeQueryResult>
{
    public async Task<GetLevyDeclarationsByAccountAndDateRangeQueryResult> Handle(GetLevyDeclarationsByAccountAndDateRangeQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var result = await dasLevyRepository.GetAccountLevyDeclarationsByDateRange(request.AccountId, request.FromDate, request.ToDate);
        return new GetLevyDeclarationsByAccountAndDateRangeQueryResult(result);
    }
}