using SFA.DAS.EmployerFinance.Web.Orchestrators;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class OrchestratorsServiceRegistrations
{
    public static IServiceCollection AddOrchestrators(this IServiceCollection services)
    {
        services.AddTransient<IEmployerAccountTransactionsOrchestrator,EmployerAccountTransactionsOrchestrator>();
        services.AddTransient<TransfersOrchestrator>();
        services.AddTransient<IAuthenticationOrchestrator, AuthenticationOrchestrator>();
        return services;
    }
}