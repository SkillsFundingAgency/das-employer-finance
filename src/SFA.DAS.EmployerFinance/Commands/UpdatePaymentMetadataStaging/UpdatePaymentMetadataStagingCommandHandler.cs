using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Validation;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;

public class UpdatePaymentMetadataStagingCommandHandler : IRequestHandler<UpdatePaymentMetadataStagingCommand, PaymentMetaDataResponse>
{
    private readonly IValidator<UpdatePaymentMetadataStagingCommand> _validator;
    private readonly IDasLevyRepository _dasLevyRepository;

    public UpdatePaymentMetadataStagingCommandHandler(IValidator<UpdatePaymentMetadataStagingCommand> validator, IDasLevyRepository dasLevyRepository)
    {
        _validator = validator;
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<PaymentMetaDataResponse> Handle(UpdatePaymentMetadataStagingCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var metadataId = await _dasLevyRepository.UpdatePaymentMetadataStaging(request.PaymentId, request.PaymentMetadataStaging);
        return new PaymentMetaDataResponse { MetadataId = metadataId, Upserted = metadataId != 0 };
    }
}