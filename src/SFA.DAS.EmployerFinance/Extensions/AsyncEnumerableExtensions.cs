namespace SFA.DAS.EmployerFinance.Extensions;

internal static class AsyncEnumerableExtensions
{
    public static async Task<bool> AnyAsync<T>(
        this IEnumerable<T> source,
        Func<T, Task<bool>> predicate)
    {
        foreach (var item in source)
        {
            if (await predicate(item)) return true;
        }
        return false;
    }
}