namespace SFA.DAS.EmployerFinance.Api.Types;

public sealed record CalendarPeriodAmount
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
}
