using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest
{
    public class BulkPaymentsIngestCommandHandler : IRequestHandler<BulkPaymentsIngestCommand, BulkPaymentsIngestResponse>
    {
        private readonly IPaymentStagingRepository _paymentStagingRepository;
        private readonly IValidator<BulkPaymentsIngestCommand> _validator;
        private readonly ILogger<BulkPaymentsIngestCommandHandler> _logger;

        public BulkPaymentsIngestCommandHandler(
            IValidator<BulkPaymentsIngestCommand> validator,
            IPaymentStagingRepository paymentStagingRepository,
            ILogger<BulkPaymentsIngestCommandHandler> logger)
        {
            _validator = validator;
            _paymentStagingRepository = paymentStagingRepository;
            _logger = logger;
        }

        public async Task<BulkPaymentsIngestResponse> Handle(BulkPaymentsIngestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = _validator.Validate(request);

                if (!validationResult.IsValid())
                {
                    return new BulkPaymentsIngestResponse
                    {
                        HasValidationErrors = true,
                        ValidationErrors = validationResult.ValidationDictionary
                            .Select(e => e.Value)
                            .ToList()
                    };
                }

                var duplicateIds = request.Payments
                    .GroupBy(x => x.PaymentId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateIds.Any())
                {
                    return new BulkPaymentsIngestResponse
                    {
                        ConflictingPaymentIds = duplicateIds
                    };
                }

                var existingIds = await _paymentStagingRepository
                    .GetExistingPaymentIds(request.Payments.Select(p => p.PaymentId).ToList());

                if (existingIds.Any())
                {
                    return new BulkPaymentsIngestResponse
                    {
                        ConflictingPaymentIds = existingIds
                    };
                }

                await _paymentStagingRepository.BulkInsertPaymentsAsync(request.Payments);

                return new BulkPaymentsIngestResponse
                {
                    IsSuccess = true,
                    InsertedCount = request.Payments.Count,
                    PaymentIds = request.Payments.Select(x => x.PaymentId).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[CorrelationId: {CorrelationId}] Exception occurred in BulkInsertPaymentsAsync.",
                    request.CorrelationId);

                return new BulkPaymentsIngestResponse
                {
                    IsSuccess = false
                };
            }
        }
    }
}