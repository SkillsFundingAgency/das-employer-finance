using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;

public class UpdatePaymentMetadataStagingCommandHandler
    : IRequestHandler<UpdatePaymentMetadataStagingCommand, PaymentMetaDataResponse>
{
    private readonly IValidator<UpdatePaymentMetadataStagingCommand> _validator;
    private readonly IDasLevyRepository _dasLevyRepository;

    public UpdatePaymentMetadataStagingCommandHandler(
        IValidator<UpdatePaymentMetadataStagingCommand> validator,
        IDasLevyRepository dasLevyRepository)
    {
        _validator = validator;
        _dasLevyRepository = dasLevyRepository;
    }

    public async Task<PaymentMetaDataResponse> Handle(
        UpdatePaymentMetadataStagingCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            return new PaymentMetaDataResponse
            {
                HasValidationErrors = true,
                ValidationErrors = validationResult.ValidationDictionary
                    .Select(e => e.Value)
                    .ToList()
            };
        }

        var paymentExists = await _dasLevyRepository.PaymentStagingExists(request.PaymentId);

        if (!paymentExists)
        {
            return new PaymentMetaDataResponse
            {
                NotFound = true
            };
        }

        var metadataId = await _dasLevyRepository.UpdatePaymentMetadataStaging(
            request.PaymentId,
            request.PaymentMetadataStaging);

        return new PaymentMetaDataResponse
        {
            IsSuccess = true,
            Upserted = true,
            MetadataId = metadataId
        };
    }
}