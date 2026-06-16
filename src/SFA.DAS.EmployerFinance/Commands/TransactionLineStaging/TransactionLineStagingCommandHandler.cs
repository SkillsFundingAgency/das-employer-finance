using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.TransactionLineStaging;

public class TransactionLineStagingCommandHandler : IRequestHandler<TransactionLineStagingCommand, TransactionLineStagingResponse>
{
    private readonly ITransactionLineStagingRepository _repository;
    private readonly IValidator<TransactionLineStagingCommand> _validator;
    private readonly ILogger<TransactionLineStagingCommandHandler> _logger;

    public TransactionLineStagingCommandHandler(
        IValidator<TransactionLineStagingCommand> validator,
        ITransactionLineStagingRepository repository,
        ILogger<TransactionLineStagingCommandHandler> logger)
    {
        _validator = validator;
        _repository = repository;
        _logger = logger;
    }

    public async Task<TransactionLineStagingResponse> Handle(TransactionLineStagingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid())
            {
                return new TransactionLineStagingResponse
                {
                    HasValidationErrors = true,
                    IsSuccess = false,
                    Message = "Validation failed.",
                    ValidationErrors = validationResult.ValidationDictionary
                        .Select(e => e.Value)
                        .ToList()
                };
            }

            await _repository.BulkInsertTransactionLinesAsync(request.TransactionLines);

            return new TransactionLineStagingResponse
            {
                IsSuccess = true,
                Message = "Transaction lines staged successfully.",
                InsertedCount = request.TransactionLines.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[CorrelationId: {CorrelationId}] Exception occurred in BulkInsertTransactionLinesAsync.",
                request.CorrelationId);

            return new TransactionLineStagingResponse
            {
                IsSuccess = false,
                Message = "An unexpected error occurred while staging transaction lines."
            };
        }
    }
}
