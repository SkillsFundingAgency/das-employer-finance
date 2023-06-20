using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Configuration;

namespace SFA.DAS.EmployerFinance.Api.ServiceRegistrations
{
    public static class CachingServiceRegistrations
    {
        public static IServiceCollection AddDasDistributedMemoryCache(this IServiceCollection services, EmployerFinanceConfiguration configuration, bool isDevelopment)
        {
            if (isDevelopment)
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(o => o.Configuration = configuration.RedisConnectionString);
            }

            return services;
        }
    }
}
