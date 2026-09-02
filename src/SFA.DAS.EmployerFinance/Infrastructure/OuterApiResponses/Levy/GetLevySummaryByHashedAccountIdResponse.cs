namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

public record GetLevySummaryByHashedAccountIdResponse
{
    public decimal CurrentLevyFunds { get; set; }
}