using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Time;

namespace SFA.DAS.EmployerFinance.Jobs.ServiceRegistrations;

public static class ApplicationServiceRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<ICurrentDateTime, CurrentDateTime>();
     
        return services;
    }
}