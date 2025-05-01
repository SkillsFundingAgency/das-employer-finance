using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class ContentApiClientWithCaching : IContentApiClient
{
    private readonly IContentApiClient _contentService;
    private readonly ICacheStorageService _cacheStorageService;
    private readonly EmployerFinanceConfiguration _configuration;

    public ContentApiClientWithCaching(
        IContentApiClient contentService, 
        ICacheStorageService cacheStorageService, 
        EmployerFinanceConfiguration configuration)
    {
        _contentService = contentService;
        _cacheStorageService = cacheStorageService;
        _configuration = configuration;
    }

    public async Task<string> Get(string type, string applicationId)
    {
        var cacheKey = $"{applicationId}_{type}".ToLowerInvariant();

        try
        {
            if (_cacheStorageService.TryGet(cacheKey, out string cachedContentBanner))
            {
                return cachedContentBanner;
            }

            var content = await _contentService.Get(type, applicationId);

            if (content != null)
            {
                await _cacheStorageService.Save(cacheKey, content, _configuration.DefaultCacheExpirationInMinutes);
            }

            return content;
        }
        catch(Exception ex)
        {
            throw new ArgumentException($"Failed to get content for {cacheKey}", ex);
        }
    }
}