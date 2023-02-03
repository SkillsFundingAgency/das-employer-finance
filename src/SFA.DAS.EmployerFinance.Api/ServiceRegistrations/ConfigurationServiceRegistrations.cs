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

            var employerAccountsConfiguration = configuration.Get<EmployerFinanceConfiguration>();
            services.AddSingleton(employerAccountsConfiguration);

            services.Configure<EncodingConfig>(configuration.GetSection(ConfigurationKeys.EncodingConfig));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EncodingConfig>>().Value);

            return services;
        }
    }
}
