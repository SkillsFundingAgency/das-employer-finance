using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;

public static class ClientServiceRegistrations
{
    public static IServiceCollection AddClientRegistrations(this IServiceCollection services)
    {
        services.AddTransient<IPaymentsEventsApiClientFactory, PaymentsEventsApiClientFactory>();
        services.AddScoped(provider =>
        {
            var factory = provider.GetService<IPaymentsEventsApiClientFactory>();
            return factory.CreateClient();
        });

        return services;
    }
}