namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

public sealed record GetLevyProjectionsByAccountIdResponse
{
    public IReadOnlyList<MonthlyBreakdown> Projections { get; set; } = [];
    public DateTime LastRefreshDateTime { get; set; }

    public sealed record MonthlyBreakdown
    {
        public int CalendarPeriodMonth { get; init; } = 0;
        public int CalendarPeriodYear { get; init; } = 0;
        public string CalendarMonthName { get; init; } = string.Empty;
        public decimal LevyIn { get; init; } = 0;
    }
}