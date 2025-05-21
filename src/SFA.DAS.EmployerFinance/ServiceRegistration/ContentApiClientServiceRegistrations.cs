using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class ContentApiClientServiceRegistrations
{
    public static IServiceCollection AddContentApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.Configure<ContentClientApiConfiguration>(configuration.GetSection(ContentClientApiConfiguration.SectionName));
        services.AddSingleton<IContentClientApiConfiguration>(cfg => cfg.GetService<IOptions<ContentClientApiConfiguration>>().Value);
        
        services.AddHttpClient<IContentApiClient, ContentApiClient>();
        services.Decorate<IContentApiClient, ContentApiClientWithCaching>();

        // Add associated services
        services.AddTransient<IAssociatedAccountsService, AssociatedAccountsService>();

        return services;
    }
}