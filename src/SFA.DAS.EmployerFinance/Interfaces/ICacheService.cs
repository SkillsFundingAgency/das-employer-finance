namespace SFA.DAS.EmployerFinance.Interfaces;

public interface ICacheService
{
    Task<(bool Found, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
