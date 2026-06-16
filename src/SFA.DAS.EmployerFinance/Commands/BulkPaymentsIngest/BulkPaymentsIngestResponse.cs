#nullable enable
namespace SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest
{
    public class BulkPaymentsIngestResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public int InsertedCount { get; set; }
        public List<Guid>? PaymentIds { get; set; }
        public List<Guid> ConflictingPaymentIds { get; set; } = new();
        public List<string> ValidationErrors { get; set; } = new();
        public bool HasConflicts { get; set; }
        public bool HasValidationErrors { get; set; }
    }
}