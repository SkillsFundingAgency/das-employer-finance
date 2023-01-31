using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Api.Client;

namespace SFA.DAS.EmployerFinance.ServiceRegistration
{
    public static class EmployerFinanceApiClientServiceRegistrations
    {
        public static IServiceCollection AddEmployerFinanceApi(this IServiceCollection services)
        {
            services.AddTransient<IEmployerFinanceApiClient,EmployerFinanceApiClient>();
            services.AddTransient<ISecureHttpClient, SecureHttpClient>();

            return services;
        }
    }
}
