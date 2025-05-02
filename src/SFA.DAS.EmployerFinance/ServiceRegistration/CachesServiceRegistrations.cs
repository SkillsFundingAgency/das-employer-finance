using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class CachesServiceRegistrations
{
    public static IServiceCollection AddCachesRegistrations(this IServiceCollection services, bool isLocal)
    {
        services.AddSingleton<ICacheStorageService, CacheStorageService>();
        services.AddSingleton<IInProcessCache, InProcessCache>();

        services.AddSingleton(s =>
        {
            var config = s.GetService<EmployerFinanceConfiguration>();

            return isLocal
                ? new LocalDevCache() as IDistributedCache
                : new RedisCache(config.RedisConnectionString);
        });

        return services;
    }
}