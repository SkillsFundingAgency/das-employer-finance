using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Authorization.EmployerFeatures.Configuration;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.Encoding;

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

            var encodingConfigJson = configuration.GetSection("SFA.DAS.Encoding").Value;
            var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
            services.AddSingleton(encodingConfig);

            return services;
        }
    }
}
