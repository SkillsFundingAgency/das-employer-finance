using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AutoConfiguration;
using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Configuration;

namespace SFA.DAS.EmployerFinance.ServiceRegistration
{
    public static class CachesServiceRegistrations
    {
        public static IServiceCollection AddCachesRegistrations(this IServiceCollection services)
        {
            services.AddSingleton<IInProcessCache, InProcessCache>();

            services.AddSingleton(s =>
            {
                var environment = s.GetService<IEnvironmentService>();
                var config = s.GetService<EmployerFinanceConfiguration>();

                return environment.IsCurrent(DasEnv.LOCAL)
                    ? new LocalDevCache() as IDistributedCache
                    : new RedisCache(config.RedisConnectionString);
            });

            return services;
        }
    }
}
