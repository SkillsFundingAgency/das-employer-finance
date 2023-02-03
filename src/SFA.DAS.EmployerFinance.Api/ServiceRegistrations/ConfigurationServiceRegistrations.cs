using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Authorization.EmployerFeatures.Configuration;
using SFA.DAS.EmployerFinance.Configuration;


namespace SFA.DAS.EmployerFinance.Api.ServiceRegistrations
{
    public static class ConfigurationServiceRegistrations
    {
        public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<EmployerFinanceConfiguration>(configuration.GetSection(ConfigurationKeys.EmployerFinance));

            var employerFinanceConfiguration = configuration.Get<EmployerFinanceConfiguration>();
            services.AddSingleton(employerFinanceConfiguration);

            return services;
        }
    }
}
