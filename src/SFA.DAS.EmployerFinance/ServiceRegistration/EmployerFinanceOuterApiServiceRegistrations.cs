using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class EmployerFinanceOuterApiServiceRegistrations
{
    public static IServiceCollection AddEmployerFinanceOuterApi(this IServiceCollection services, EmployerFinanceOuterApiConfiguration outerApiConfiguration)
    {
        services.AddHttpClient<IOuterApiClient, OuterApiClient>(x =>
        {
            x.BaseAddress = new Uri(outerApiConfiguration.BaseUrl);
            x.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", outerApiConfiguration.Key);
            x.DefaultRequestHeaders.Add("X-Version", "1");
        });

        return services;
    }
}