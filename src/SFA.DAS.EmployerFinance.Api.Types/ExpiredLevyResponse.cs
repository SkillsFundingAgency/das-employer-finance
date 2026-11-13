namespace SFA.DAS.EmployerFinance.Api.Types;

public sealed record ExpiredLevyResponse
{
    /// <summary>ExpiredFund (transaction type 5) - the 24-month expiry.</summary>
    public List<CalendarPeriodAmount> LongTerm { get; init; } = [];

    /// <summary>ShortExpiredFund (transaction type 6) - the 12-month expiry.</summary>
    public List<CalendarPeriodAmount> ShortTerm { get; init; } = [];
}
