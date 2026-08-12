using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;

public class GetLevyDeclarationSubmissionIdsQueryHandler
    : IRequestHandler<GetLevyDeclarationSubmissionIdsQuery, List<long>>
{
    private readonly IValidator<GetLevyDeclarationSubmissionIdsQuery> _validator;
    private readonly IDasLevyRepository _dasLevyRepository;

    public GetLevyDeclarationSubmissionIdsQueryHandler(
        IValidator<GetLevyDeclarationSubmissionIdsQuery> validator,
        IDasLevyRepository dasLevyRepository)
    {
        _validator = validator;
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<List<long>> Handle(
        GetLevyDeclarationSubmissionIdsQuery message,
        CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var ids = await _dasLevyRepository.GetEmployerDeclarationSubmissionIds(message.EmpRef);
        return ids.ToList();
    }
}
