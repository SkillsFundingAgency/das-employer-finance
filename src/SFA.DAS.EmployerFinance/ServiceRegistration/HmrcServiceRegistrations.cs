using SFA.DAS.ActiveDirectory;
using SFA.DAS.EmployerFinance.Http;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class HmrcServiceRegistrations
{
    public static IServiceCollection AddHmrcServices(this IServiceCollection services)
    {
        services.AddSingleton<IHmrcService, HmrcService>();
        services.AddSingleton<IHttpResponseLogger, HttpResponseLogger>();
        services.AddSingleton<ITokenServiceApiClient, TokenServiceApiClient>();
        services.AddSingleton<IAzureAdAuthenticationService, AzureAdAuthenticationService>();
        services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();

        return services;
    }
}