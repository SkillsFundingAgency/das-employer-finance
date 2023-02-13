using AutoMapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Configuration;
using StackExchange.Redis;

namespace SFA.DAS.EmployerFinance.ServiceRegistration
{
    public static class DataProtectionServiceRegistrations
    {
        public static IServiceCollection AddDataProtection(IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection(nameof(EmployerFinanceConfiguration))
          .Get<EmployerFinanceConfiguration>();

            if (config != null
                && !string.IsNullOrEmpty(config.DataProtectionKeysDatabase)
                && !string.IsNullOrEmpty(config.RedisConnectionString))
            {
                var redisConnectionString = config.RedisConnectionString;
                var dataProtectionKeysDatabase = config.DataProtectionKeysDatabase;

                var redis = ConnectionMultiplexer
                    .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

                services.AddDataProtection()
                    .SetApplicationName("das-forecasting-web")
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }

            return services;
        }
    }
}
