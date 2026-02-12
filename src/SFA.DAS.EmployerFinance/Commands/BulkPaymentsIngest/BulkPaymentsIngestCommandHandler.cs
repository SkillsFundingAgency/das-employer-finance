using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest
{
    public class BulkPaymentsIngestCommandHandler : IRequestHandler<BulkPaymentsIngestCommand, BulkPaymentsIngestResponse>
    {
        private readonly IPaymentStagingRepository _paymentStagingRepository;
        private readonly ILogger<BulkPaymentsIngestCommandHandler> _logger;

        public BulkPaymentsIngestCommandHandler(IPaymentStagingRepository paymentStagingRepository, ILogger<BulkPaymentsIngestCommandHandler> logger)
        {
            _paymentStagingRepository = paymentStagingRepository;
            _logger = logger;
        }

        public async Task<BulkPaymentsIngestResponse> Handle(BulkPaymentsIngestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _paymentStagingRepository.BulkInsertPaymentsAsync(request.Payments);

                return new BulkPaymentsIngestResponse
                {
                    IsSuccess = result.IsSuccess,
                    InsertedCount = result.InsertedCount,
                    PaymentIds = result.PaymentIds,
                    Message = result.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[CorrelationId: {request.CorrelationId}] Exception occured in BulkInsertPaymentsAsync.");
                throw;
            }
        }
    }
}