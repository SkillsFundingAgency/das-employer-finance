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

        services.Configure<EmployerFinanceConfiguration>(configuration.GetSection(ConfigurationKeys.EmployerFinance));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFinanceConfiguration>>().Value);
        services.AddSingleton(configuration.GetSection(ConfigurationKeys.EmployerFinance).Get<EmployerFinanceConfiguration>());

        services.Configure<IPaymentsEventsApiClientConfiguration>(configuration.GetSection(ConfigurationKeys.PaymentEventsApiClient));
        services.AddSingleton<IPaymentsEventsApiClientConfiguration>(configuration.GetSection(ConfigurationKeys.PaymentEventsApiClient).Get<PaymentsEventsApiClientConfiguration>());

        services.Configure<PaymentsEventsApiClientLocalConfiguration>(configuration.GetSection(ConfigurationKeys.PaymentEventsApiClient));
        services.AddSingleton(configuration.GetSection(ConfigurationKeys.PaymentEventsApiClient).Get<PaymentsEventsApiClientLocalConfiguration>());

        services.Configure<CommitmentsApiV2ClientConfiguration>(configuration.GetSection(ConfigurationKeys.CommitmentsV2ApiClient));
        services.AddSingleton(configuration.GetSection(ConfigurationKeys.CommitmentsV2ApiClient).Get<CommitmentsApiV2ClientConfiguration>());

        var encodingConfigJson = configuration.GetSection("SFA.DAS.Encoding").Value;
        var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
        services.AddSingleton(encodingConfig);

        var financeConfiguration = configuration
            .GetSection(ConfigurationKeys.EmployerFinance)
            .Get<EmployerFinanceConfiguration>();

        services.AddSingleton(financeConfiguration);

        services.AddSingleton<IHmrcConfiguration>(_ => financeConfiguration.Hmrc);
        services.AddSingleton<ITokenServiceApiClientConfiguration>(_ => financeConfiguration.TokenServiceApi);
        services.AddSingleton<EmployerFinanceOuterApiConfiguration>(_ => financeConfiguration.EmployerFinanceOuterApiConfiguration);
        ;


        return services;
    }
}