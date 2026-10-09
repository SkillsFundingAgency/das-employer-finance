namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

public sealed record GetLevyProjectionsByAccountIdResponse
{
    public IReadOnlyList<MonthlyBreakdown> Projections { get; init; } = [];
    public DateTime LastRefreshDateTime { get; set; }

    public sealed record MonthlyBreakdown
    {
        public int CalendarPeriodMonth { get; init; } = 0;
        public int CalendarPeriodYear { get; init; } = 0;
        public string CalendarMonthName { get; init; } = string.Empty;
        public decimal LevyIn { get; init; } = 0;
        public decimal LevyOut { get; init; } = 0;
        public decimal ClosingLevyBalance { get; init; } = 0;
        public decimal ExpiredLevy { get; init; } = 0;
        public decimal CommittedLearnerCosts { get; init; } = 0;
        public decimal CommittedTransferCosts { get; init; } = 0;
    }
}