using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class ContentApiClientWithCaching(
    IContentApiClient contentService,
    ICacheService cacheService,
    EmployerFinanceConfiguration configuration)
    : IContentApiClient
{
    public async Task<string> Get(string type, string applicationId)
    {
        var cacheKey = $"{applicationId}_{type}".ToLowerInvariant();

        try
        {
            var (found, cached) = await cacheService.TryGetAsync<string>(cacheKey);
            if (found && cached != null)
            {
                return cached;
            }

            var content = await contentService.Get(type, applicationId);

            if (content != null)
            {
                var expiration = TimeSpan.FromMinutes(configuration.DefaultCacheExpirationInMinutes);
                await cacheService.SetAsync(cacheKey, content, expiration);
            }

            return content;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to get content for {cacheKey}", ex);
        }
    }
}