using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.Encoding;
using SFA.DAS.Provider.Events.Api.Client.Configuration;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;

public static class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddConfigurationSections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.Configure<EmployerFinanceJobsConfiguration>(configuration.GetSection(nameof(EmployerFinanceJobsConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFinanceJobsConfiguration>>().Value);

        services.AddSingleton<EmployerFinanceConfiguration>(sp => sp.GetService<EmployerFinanceJobsConfiguration>());

        services.Configure<IPaymentsEventsApiClientConfiguration>(configuration.GetSection(ConfigurationKeys.PaymentEventsApiClient));
        services.AddSingleton<IPaymentsEventsApiClientConfiguration>(configuration.GetSection(ConfigurationKeys.PaymentEventsApiClient).Get<PaymentsEventsApiClientConfiguration>());

        services.Configure<PaymentsEventsApiClientLocalConfiguration>(configuration.GetSection(ConfigurationKeys.PaymentEventsApiClient));
        services.AddSingleton(configuration.GetSection(ConfigurationKeys.PaymentEventsApiClient).Get<PaymentsEventsApiClientLocalConfiguration>());

        services.Configure<TokenServiceApiClientConfiguration>(configuration.GetSection(ConfigurationKeys.TokenServiceApi));
        services.AddSingleton<ITokenServiceApiClientConfiguration>(cfg => cfg.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value);
        
        services.Configure<HmrcConfiguration>(configuration.GetSection(ConfigurationKeys.Hmrc));
        services.AddSingleton<IHmrcConfiguration>(cfg => cfg.GetService<IOptions<HmrcConfiguration>>().Value);

        services.Configure<CommitmentsApiV2ClientConfiguration>(configuration.GetSection(nameof(CommitmentsApiV2ClientConfiguration)));
        services.AddSingleton(configuration.GetSection(nameof(CommitmentsApiV2ClientConfiguration)).Get<CommitmentsApiV2ClientConfiguration>());

        var encodingConfigJson = configuration.GetSection(ConfigurationKeys.EncodingConfig).Value;
        var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
        services.AddSingleton(encodingConfig);
        
        var outerApiConfiguration = configuration
            .GetSection(nameof(EmployerFinanceOuterApiConfiguration))
            .Get<EmployerFinanceOuterApiConfiguration>();

        services.AddSingleton(outerApiConfiguration);

        return services;
    }
}