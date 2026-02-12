#nullable enable
namespace SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest
{
    public class BulkPaymentsIngestResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public int InsertedCount { get; set; }
        public List<Guid>? PaymentIds { get; set; }
    }
}