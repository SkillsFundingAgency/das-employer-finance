using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class CachesServiceRegistrations
{
    public static IServiceCollection AddCachesRegistrations(
        this IServiceCollection services,
        string redisConnectionString,
        bool isLocal)
    {
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IInProcessCache, InProcessCache>();

        if (isLocal)
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(o => o.Configuration = redisConnectionString);
        }

        return services;
    }
}