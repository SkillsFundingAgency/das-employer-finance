using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;

public static class ClientServiceRegistrations
{
    public static IServiceCollection AddClientRegistrations(this IServiceCollection services)
    {
        services.AddTransient<IPaymentsEventsApiClientFactory, PaymentsEventsApiClientFactory>();
        services.AddTransient<IPaymentsEventsApiClient>(provider => provider.GetService<IPaymentsEventsApiClientFactory>().CreateClient());

        return services;
    }
}