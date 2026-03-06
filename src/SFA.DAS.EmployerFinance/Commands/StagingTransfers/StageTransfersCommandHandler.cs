using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class StageTransfersCommandHandler : IRequestHandler<StageTransfersCommand, StageTransfersResponse>
{
    private readonly ITransferStagingRepository _transferStagingRepository;
    private readonly IValidator<StageTransfersCommand> _validator;

    public StageTransfersCommandHandler(IValidator<StageTransfersCommand> validator, ITransferStagingRepository transferStagingRepository)
    {
        _validator = validator;
        _transferStagingRepository = transferStagingRepository;
    }

    public async Task<StageTransfersResponse> Handle(StageTransfersCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var duplicateIds = request.Transfers
        .GroupBy(x => x.TransferId)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key)
        .ToList();

        if (duplicateIds.Any())
        {
            return new StageTransfersResponse
            {
                ConflictingTransferIds = duplicateIds
            };
        }

        var existingIds = await _transferStagingRepository.GetExistingTransferIds(request.Transfers.Select(x => x.TransferId).ToList());

        if (existingIds.Any())
        {
            return new StageTransfersResponse
            {
                ConflictingTransferIds = existingIds
            };
        }

        await _transferStagingRepository.BulkInsertTransfers(request.Transfers);

        return new StageTransfersResponse
        {
            IsSuccess = true,
            InsertedTransferIds = request.Transfers.Select(x => x.TransferId).ToList()
        };
    }
}
