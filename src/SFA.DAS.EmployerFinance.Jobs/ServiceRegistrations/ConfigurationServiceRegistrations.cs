using Microsoft.Extensions.Options;
using SFA.DAS.EmployerFinance.Configuration;

namespace SFA.DAS.EmployerFinance.Jobs.ServiceRegistrations;

public static class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddConfigurationSections(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmployerFinanceConfiguration>(configuration.GetSection(ConfigurationKeys.EmployerFinance));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFinanceConfiguration>>().Value);
        services.AddSingleton(configuration.GetSection(ConfigurationKeys.EmployerFinance).Get<EmployerFinanceConfiguration>());

        return services;
    }
}