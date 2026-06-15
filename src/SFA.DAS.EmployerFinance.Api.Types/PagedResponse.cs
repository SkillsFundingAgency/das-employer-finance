namespace SFA.DAS.EmployerFinance.Api.Types;

public class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();

    public int TotalCount { get; set; }
}
