using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.TransferStagedToOperational;

public class TransferStagedToOperationalCommandHandler(
    IValidator<TransferStagedToOperationalCommand> validator,
    IDasLevyRepository dasLevyRepository,
    ILogger<TransferStagedToOperationalCommandHandler> logger)
    : IRequestHandler<TransferStagedToOperationalCommand, TransferStagedToOperationalResponse>
{
    public async Task<TransferStagedToOperationalResponse> Handle(
        TransferStagedToOperationalCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            return new TransferStagedToOperationalResponse
            {
                HasValidationErrors = true,
                ValidationErrors = validationResult.ValidationDictionary
                    .Select(error => error.Value)
                    .ToList()
            };
        }

        logger.LogInformation(
            "Transferring staged data to operational for AccountId {AccountId}, PeriodEnd {PeriodEnd}, CorrelationId {CorrelationId}",
            request.AccountId,
            request.PeriodEnd,
            request.CorrelationId);

        try
        {
            var processedCount = await dasLevyRepository.TransferStagedToOperational(
                request.AccountId,
                request.PeriodEnd);

            logger.LogInformation(
                "Transferred staged data to operational for AccountId {AccountId}, PeriodEnd {PeriodEnd}, CorrelationId {CorrelationId}. ProcessedCount {ProcessedCount}",
                request.AccountId,
                request.PeriodEnd,
                request.CorrelationId,
                processedCount);

            return new TransferStagedToOperationalResponse
            {
                IsSuccess = true,
                ProcessedCount = processedCount,
                Message = $"Successfully transferred {processedCount} staged rows to operational."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed transferring staged data to operational for AccountId {AccountId}, PeriodEnd {PeriodEnd}, CorrelationId {CorrelationId}",
                request.AccountId,
                request.PeriodEnd,
                request.CorrelationId);

            return new TransferStagedToOperationalResponse
            {
                IsSuccess = false,
                Message = "An unexpected error occurred while transferring staged data to operational."
            };
        }
    }
}
