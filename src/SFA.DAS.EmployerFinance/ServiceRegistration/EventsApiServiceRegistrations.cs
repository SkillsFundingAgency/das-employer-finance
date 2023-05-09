using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Client.Configuration;
using SFA.DAS.UnitOfWork.Pipeline;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class EventsApiServiceRegistrations
{
    public static IServiceCollection AddEventsApi(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork.NServiceBus.Pipeline.UnitOfWork>();
        services.AddTransient<IEventsApi>(s =>
        {
            var config = s.GetService<IEventsApiClientConfiguration>();
            return new EventsApi(config);
        });

        return services;
    }
}