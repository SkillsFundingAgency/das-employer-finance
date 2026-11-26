using System.Net.Http;
using HMRC.ESFA.Levy.Api.Client;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.MessageHandlers.Http;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Provider.Events.Api.Client;

namespace SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;

public static class ClientServiceRegistrations
{
    private const int HmrcMaxRequestsPerWindow = 5;
    private static readonly TimeSpan HmrcRateLimitWindow = TimeSpan.FromSeconds(2);

    public static IServiceCollection AddClientRegistrations(this IServiceCollection services)
    {
        services.AddTransient<IPaymentsEventsApiClientFactory, PaymentsEventsApiClientFactory>();
        services.AddTransient(provider => provider.GetService<IPaymentsEventsApiClientFactory>().CreateClient());

        services.AddHttpClient<ICommitmentsV2ApiClient, CommitmentsV2ApiClient>();

        services.AddSingleton<IHmrcRateLimiter>(_ => new SlidingWindowHmrcRateLimiter(HmrcMaxRequestsPerWindow, HmrcRateLimitWindow));

        services.AddTransient<IApprenticeshipLevyApiClient>(serviceProvider =>
        {
            var rateLimiter = serviceProvider.GetRequiredService<IHmrcRateLimiter>();
            var logger = serviceProvider.GetRequiredService<ILogger<HmrcRateLimitingHandler>>();
            var socketsHandler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) };
            var rateLimitingHandler = new HmrcRateLimitingHandler(rateLimiter, logger, socketsHandler);
            var client = new HttpClient(rateLimitingHandler)
            {
                BaseAddress = new Uri(serviceProvider.GetService<IHmrcConfiguration>().BaseUrl)
            };
            return new ApprenticeshipLevyApiClient(client);
        });

        return services;
    }
}