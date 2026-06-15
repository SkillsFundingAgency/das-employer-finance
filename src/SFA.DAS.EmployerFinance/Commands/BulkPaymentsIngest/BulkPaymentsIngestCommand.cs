using SFA.DAS.EmployerFinance.Models.PaymentStaging;

namespace SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest
{
    public class BulkPaymentsIngestCommand : IRequest<BulkPaymentsIngestResponse>
    {
        public string CorrelationId { get; set; }
        public List<PaymentStagingModel> Payments { get; set; }
    }
}