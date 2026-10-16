namespace SFA.DAS.EmployerFinance.Api.Types;

public sealed record PaymentResponse
{
    public Guid Id { get; set; }
    public long Ukprn { get; set; }
    public long Uln { get; set; }
    public long EmployerAccountId { get; set; }
    public long ApprenticeshipId { get; set; }
    public int DeliveryPeriodMonth { get; set; }
    public int DeliveryPeriodYear { get; set; }
    public string CollectionPeriodId { get; set; }
    public int CollectionPeriodMonth { get; set; }
    public int CollectionPeriodYear { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateImported { get; set; }
}