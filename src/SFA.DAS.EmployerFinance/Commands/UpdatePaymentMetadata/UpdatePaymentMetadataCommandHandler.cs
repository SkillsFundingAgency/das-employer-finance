using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadata;

public class UpdatePaymentMetadataCommandHandler : IRequestHandler<UpdatePaymentMetadataCommand>
{
    private readonly IValidator<UpdatePaymentMetadataCommand> _validator;
    private readonly IDasLevyRepository _dasLevyRepository;

    public UpdatePaymentMetadataCommandHandler(IValidator<UpdatePaymentMetadataCommand> validator, IDasLevyRepository dasLevyRepository)
    {
        _validator = validator;
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task Handle(UpdatePaymentMetadataCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        await _dasLevyRepository.UpdatePaymentMetadata(request.PaymentId, request.PaymentMetadata);
    }
}
