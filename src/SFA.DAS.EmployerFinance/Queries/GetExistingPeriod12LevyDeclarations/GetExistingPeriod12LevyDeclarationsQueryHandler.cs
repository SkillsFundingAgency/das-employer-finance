using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;

public class GetExistingPeriod12LevyDeclarationsQueryHandler
    : IRequestHandler<GetExistingPeriod12LevyDeclarationsQuery, List<ExistingPeriod12LevyDeclarationResult>>
{
    private const short Period12PayrollMonth = 12;

    private readonly IValidator<GetExistingPeriod12LevyDeclarationsQuery> _validator;
    private readonly IDasLevyRepository _dasLevyRepository;
    private readonly ICurrentDateTime _currentDateTime;

    public GetExistingPeriod12LevyDeclarationsQueryHandler(
        IValidator<GetExistingPeriod12LevyDeclarationsQuery> validator,
        IDasLevyRepository dasLevyRepository,
        ICurrentDateTime currentDateTime)
    {
        _validator = validator;
        _dasLevyRepository = dasLevyRepository;
        _currentDateTime = currentDateTime;
    }

    public async Task<List<ExistingPeriod12LevyDeclarationResult>> Handle(
        GetExistingPeriod12LevyDeclarationsQuery message,
        CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var payrollYear = _currentDateTime.Now.ToPayrollYearString();

        return await _dasLevyRepository.GetExistingPeriod12LevyDeclarations(
            message.EmpRef,
            payrollYear,
            Period12PayrollMonth);
    }
}
