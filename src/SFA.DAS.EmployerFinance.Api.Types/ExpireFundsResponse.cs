namespace SFA.DAS.EmployerFinance.Api.Types;

public class ExpireFundsResponse
{
    public long AccountId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public bool FundsExpired { get; set; }
    public int LongTermExpiredFundsCount { get; set; }
    public int ShortTermExpiredFundsCount { get; set; }
}
