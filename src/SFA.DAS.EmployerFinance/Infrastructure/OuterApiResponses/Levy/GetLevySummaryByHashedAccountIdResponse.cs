namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

public record GetLevySummaryByHashedAccountIdResponse
{
    public decimal CurrentLevyFunds { get; set; }
    public decimal TotalLevyDeclaredLast12Months { get; set; }
    public decimal TotalLevySpentLast12Months { get; set; }
}