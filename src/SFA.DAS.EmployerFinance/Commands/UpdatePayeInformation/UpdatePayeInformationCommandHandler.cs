using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Hmrc;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;

public class UpdatePayeInformationCommandHandler : IRequestHandler<UpdatePayeInformationCommand, Unit>
{
    private readonly IValidator<UpdatePayeInformationCommand> _validator;
    private readonly IPayeRepository _payeRepository;
    private readonly IHmrcService _hmrcService;

    public UpdatePayeInformationCommandHandler(IValidator<UpdatePayeInformationCommand> validator, IPayeRepository payeRepository, IHmrcService hmrcService)
    {
        _validator = validator;
        _payeRepository = payeRepository;
        _hmrcService = hmrcService;
    }

    public async Task<Unit> Handle(UpdatePayeInformationCommand request,CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var scheme = await _payeRepository.GetPayeSchemeByRef(request.PayeRef);

        if (!string.IsNullOrEmpty(scheme?.Name))
        {
            return Unit.Value;
        }

        var result = await _hmrcService.GetEmprefInformation(scheme?.EmpRef);

        if (string.IsNullOrEmpty(result?.Employer?.Name?.EmprefAssociatedName))
        {
            return Unit.Value;
        }

        await _payeRepository.UpdatePayeSchemeName(request.PayeRef, result.Employer.Name.EmprefAssociatedName);

        return Unit.Value;
    }
}