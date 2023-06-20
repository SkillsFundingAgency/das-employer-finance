using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Time;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class DateTimeServiceRegistrations
{
    public static IServiceCollection AddDateTimeServices(this IServiceCollection services, IConfiguration configuration)
    {
        var cloudCurrentTime = configuration.GetValue<string>("CurrentTime");

        if(!DateTime.TryParse(cloudCurrentTime, out var currentTime))
        {
            currentTime= DateTime.Now;
        }

        services.AddSingleton<ICurrentDateTime>(_ => new CurrentDateTime(currentTime));

        return services;
    }

}