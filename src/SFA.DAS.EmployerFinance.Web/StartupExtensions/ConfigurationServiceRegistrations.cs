using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Audit.Client;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization.EmployerFeatures.Configuration;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerAccounts.ReadStore.Configuration;
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
            
            services.Configure<EmployerAccountsReadStoreConfiguration>(configuration.GetSection(nameof(EmployerAccountsReadStoreConfiguration)));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountsReadStoreConfiguration>>().Value);

            services.Configure<EmployerFeaturesConfiguration>(configuration.GetSection(ConfigurationKeys.Features));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFeaturesConfiguration>>().Value);

            services.Configure<IAccountApiConfiguration>(configuration.GetSection(nameof(AccountApiConfiguration)));
            services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>();

            services.Configure<IdentityServerConfiguration>(configuration.GetSection("Identity"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<IdentityServerConfiguration>>().Value);

            //services.Configure<IAuditApiConfiguration>(configuration.GetSection(ConfigurationKeys.AuditApi));
            //services.AddSingleton(cfg => cfg.GetService<IOptions<IAuditApiConfiguration>>().Value);

            services.Configure<EncodingConfig>(configuration.GetSection(ConfigurationKeys.EncodingConfig));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EncodingConfig>>().Value);


            //services.Configure<ITokenServiceApiClientConfiguration>(configuration.GetSection(nameof(TokenServiceApiClientConfiguration)));

            var employerFinanceConfiguration = configuration.GetSection(nameof(EmployerFinanceConfiguration)).Get<EmployerFinanceConfiguration>();

            //services.AddSingleton<IHmrcConfiguration>(_ => employerAccountsConfiguration.Hmrc);
            //services.AddSingleton<ITokenServiceApiClientConfiguration>(_ => employerAccountsConfiguration.TokenServiceApi);
            //services.AddSingleton<ITaskApiConfiguration>(_ => employerAccountsConfiguration.TasksApi);
            services.AddSingleton<CommitmentsApiV2ClientConfiguration>(_ => employerFinanceConfiguration.CommitmentsApi);
            //services.AddSingleton<IProviderRegistrationClientApiConfiguration>(_ => employerAccountsConfiguration.ProviderRegistrationsApi);
            services.AddSingleton<EmployerFinanceOuterApiConfiguration>(_ => employerFinanceConfiguration.EmployerFinanceOuterApiConfiguration);

            //services.Configure<IEmployerAccountsApiClientConfiguration>(configuration.GetSection(ConfigurationKeys.EmployerAccountsApiClient));
            //services.AddSingleton<IEmployerAccountsApiClientConfiguration>(cfg => cfg.GetService<IOptions<EmployerAccountsApiClientConfiguration>>().Value);
            var encodingConfigJson = configuration.GetSection("SFA.DAS.Encoding").Value;
            var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
            services.AddSingleton(encodingConfig);
            return services;
        }
    }
}
