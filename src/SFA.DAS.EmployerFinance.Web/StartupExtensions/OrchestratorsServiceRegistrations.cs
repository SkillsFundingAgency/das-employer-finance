using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Web.Orchestrators;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions
{
    public static class OrchestratorsServiceRegistrations
    {
        public static IServiceCollection AddOrchestrators(this IServiceCollection services)
        {
            services.AddTransient<AuthenticationOrchestrator>();
            services.AddTransient<EmployerAccountTransactionsOrchestrator>();
            services.AddTransient<HomeOrchestrator>();
            services.AddTransient<TransfersOrchestrator>();
            return services;
        }
    }
}
