using HMRC.ESFA.Levy.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;

public static class ClientServiceRegistrations
{
    public static IServiceCollection AddClientRegistrations(this IServiceCollection services)
    {
        services.AddTransient<IPaymentsEventsApiClientFactory, PaymentsEventsApiClientFactory>();
        services.AddTransient<IPaymentsEventsApiClient>(provider => provider.GetService<IPaymentsEventsApiClientFactory>().CreateClient());

        services.AddHttpClient<ICommitmentsV2ApiClient, CommitmentsV2ApiClient>();
        services.AddTransient<ICommitmentsV2ApiClient, CommitmentsV2ApiClient>();

        services.AddHttpClient<IApprenticeshipLevyApiClient, ApprenticeshipLevyApiClient>((provider, client) =>
        {
            var financeConfiguration = provider.GetService<EmployerFinanceConfiguration>();
            client.BaseAddress = new Uri(financeConfiguration.Hmrc.BaseUrl);
        });

        services.AddTransient<IApprenticeshipLevyApiClient, ApprenticeshipLevyApiClient>();

        return services;
    }
}