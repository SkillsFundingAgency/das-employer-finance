using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions
{
    public static class ConfigurationServiceRegistrations
    {
        public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration.Get<EmployerFinanceConfiguration>());
            
            services.Configure<EmployerFinanceConfiguration>(configuration.GetSection(nameof(EmployerFinanceConfiguration)));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFinanceConfiguration>>().Value);
            
            services.Configure<IAccountApiConfiguration>(configuration.GetSection(nameof(AccountApiConfiguration)));
            services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>();
            

            var employerFinanceConfiguration = configuration.GetSection(nameof(EmployerFinanceConfiguration)).Get<EmployerFinanceConfiguration>();

            services.AddSingleton<CommitmentsApiV2ClientConfiguration>(_ => employerFinanceConfiguration.CommitmentsApi);
            services.AddSingleton<EmployerFinanceOuterApiConfiguration>(_ => employerFinanceConfiguration.EmployerFinanceOuterApiConfiguration);

            var encodingConfigJson = configuration.GetSection(ConfigurationKeys.EncodingConfig).Value;
            var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
            services.AddSingleton(encodingConfig);
            return services;
        }
    }
}
