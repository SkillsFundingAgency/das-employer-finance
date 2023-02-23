using Microsoft.Extensions.Options;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.Provider.Events.Api.Client.Configuration;

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

        return services;
    }
}