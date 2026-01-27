namespace SFA.DAS.EmployerFinance.Models.PaymentStaging
{
    public class PaymentStagingModel : Entity
    {
        public Guid PaymentId { get; set; }
        public long AccountId { get; set; }
        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public long ApprenticeshipId { get; set; }

        // Format: "RXX-YYYY"
        public string CollectionPeriodId { get; set; } = string.Empty;

        public int DeliveryPeriodMonth { get; set; }
        public int DeliveryPeriodYear { get; set; }
        public int CollectionPeriodMonth { get; set; }
        public int CollectionPeriodYear { get; set; }

        public string FundingSource { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime EvidenceSubmittedOn { get; set; }

        public string EmployerAccountVersion { get; set; } = string.Empty;
        public string ApprenticeshipVersion { get; set; } = string.Empty;
    }
}