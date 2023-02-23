using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Time;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;

public static class ApplicationServiceRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<ICurrentDateTime, CurrentDateTime>();
        services.AddTransient<IExpiredFunds, ExpiredFunds>();
        services.AddTransient<IDasAccountService, DasAccountService>();

        return services;
    }
}