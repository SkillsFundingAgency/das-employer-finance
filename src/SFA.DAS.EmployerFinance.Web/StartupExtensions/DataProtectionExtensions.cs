using Microsoft.AspNetCore.DataProtection;
using SFA.DAS.EmployerFinance.Configuration;
using StackExchange.Redis;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class AddDataProtectionExtensions
{
    public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
            
        var config = configuration.GetSection(nameof(EmployerFinanceConfiguration)).Get<EmployerFinanceWebConfiguration>();

        if (config == null
            || string.IsNullOrEmpty(config.DataProtectionKeysDatabase)
            || string.IsNullOrEmpty(config.RedisConnectionString))
        {
            return services;
        }


        var redisConnectionString = config.RedisConnectionString;
        var dataProtectionKeysDatabase = config.DataProtectionKeysDatabase;

        var redis = ConnectionMultiplexer
            .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

        services.AddDataProtection()
            .SetApplicationName("das-employer")
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

        return services;
    }
}