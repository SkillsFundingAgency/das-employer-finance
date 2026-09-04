namespace SFA.DAS.EmployerFinance.Api.Types;

public sealed record LevySummary
{
    public decimal CurrentLevyFunds { get; init; }
    public decimal TotalLevyDeclaredLast12Months { get; init; }
    public decimal TotalLevySpentLast12Months { get; init; }
}