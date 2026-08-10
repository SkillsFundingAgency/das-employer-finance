using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLastEnglishFractionCalculationDate;

public class GetLastEnglishFractionCalculationDateQueryHandler
    : IRequestHandler<GetLastEnglishFractionCalculationDateQuery, GetLastEnglishFractionCalculationDateResponse>
{
    private readonly IValidator<GetLastEnglishFractionCalculationDateQuery> _validator;
    private readonly IEnglishFractionRepository _englishFractionRepository;

    public GetLastEnglishFractionCalculationDateQueryHandler(
        IValidator<GetLastEnglishFractionCalculationDateQuery> validator,
        IEnglishFractionRepository englishFractionRepository)
    {
        _validator = validator;
        _englishFractionRepository = englishFractionRepository;
    }

    public async Task<GetLastEnglishFractionCalculationDateResponse> Handle(
        GetLastEnglishFractionCalculationDateQuery message,
        CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var dateCalculated = await _englishFractionRepository.GetLastStoredCalculationDateForEmpRef(message.EmpRef);

        return new GetLastEnglishFractionCalculationDateResponse
        {
            DateCalculated = dateCalculated
        };
    }
}
