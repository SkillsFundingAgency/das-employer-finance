namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

public record GetLevySummaryByAccountIdResponse
{
    public decimal CurrentLevyFunds { get; set; }
    public decimal TotalLevyDeclaredLast12Months { get; set; }
    public decimal TotalLevySpentLast12Months { get; set; }
}