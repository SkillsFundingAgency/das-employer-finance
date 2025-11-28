using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Api.Orchestrators;

namespace SFA.DAS.EmployerFinance.Api.ServiceRegistrations
{
    public static class OrchestratorsServiceRegistrations
    {   
        public static IServiceCollection AddOrchestrators(this IServiceCollection services)
        {
            services.AddTransient<AccountTransactionsOrchestrator>();
            services.AddTransient<FinanceOrchestrator>();
            services.AddTransient<StatisticsOrchestrator>();
            services.AddTransient<PeriodEndOrchestrator>();

            return services;
        }
    }
}
