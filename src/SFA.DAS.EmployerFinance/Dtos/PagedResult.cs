namespace SFA.DAS.EmployerFinance.Dtos;

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();

    public int TotalCount { get; set; }
}
