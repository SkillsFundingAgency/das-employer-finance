using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class ProviderServiceRegistration
{
    public static IServiceCollection AddProviderServices(this IServiceCollection services)
    {
        services.AddTransient<IProviderService, ProviderServiceFromDb>();
        services.Decorate<IProviderService, ProviderServiceRemote>();
        services.Decorate<IProviderService, ProviderServiceCache>();

        return services;
    }
}