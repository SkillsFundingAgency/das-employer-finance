using Microsoft.Extensions.Options;
using SFA.DAS.EmployerFinance.Configuration;

namespace SFA.DAS.EmployerFinance.Jobs.ServiceRegistrations;

public static class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddConfigurationSections(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmployerFinanceJobsConfiguration>(configuration.GetSection(nameof(EmployerFinanceJobsConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFinanceJobsConfiguration>>().Value);

        services.AddSingleton<EmployerFinanceConfiguration>(sp => sp.GetService<EmployerFinanceJobsConfiguration>());

        return services;
    }
}