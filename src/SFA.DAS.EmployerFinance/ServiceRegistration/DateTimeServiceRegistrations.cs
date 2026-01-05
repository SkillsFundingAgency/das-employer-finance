using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Time;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class DateTimeServiceRegistrations
{
    public static void AddDateTimeServices(this IServiceCollection services, IConfiguration configuration)
    {
        var cloudCurrentTime = configuration.GetValue<string>("CurrentTime");

        if(DateTime.TryParse(cloudCurrentTime, out var currentTime))
        {
            services.AddScoped<ICurrentDateTime>(_ => new CurrentDateTime(currentTime));
        }
        else
        {
            services.AddScoped<ICurrentDateTime>(_ => new CurrentDateTime());
        }
    }
}