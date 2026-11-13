namespace SFA.DAS.EmployerFinance.Api.Types;

public sealed record CollectionPeriodPaymentsResponse
{
    public bool PeriodEndCompleted { get; set; }
    public List<CollectionPeriodPaymentTotalResponse> Payments { get; set; } = [];
}

public sealed record CollectionPeriodPaymentTotalResponse
{
    public int FundingSource { get; set; }
    public int TransactionType { get; set; }
    public decimal Amount { get; set; }
}
