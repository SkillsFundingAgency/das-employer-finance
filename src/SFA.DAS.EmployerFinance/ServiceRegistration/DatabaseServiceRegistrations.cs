using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;

namespace SFA.DAS.EmployerFinance.ServiceRegistration
{
    public static class DatabaseServiceRegistrations
    {
        public static IServiceCollection AddDatabaseRegistration(this IServiceCollection services, EmployerFinanceConfiguration config, string environmentName)
        {
            if (environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddDbContext<EmployerFinanceDbContext>(options => options.UseSqlServer(config.DatabaseConnectionString), ServiceLifetime.Transient);                
            }
            else
            {
                services.AddDbContext<EmployerFinanceDbContext>(ServiceLifetime.Transient);
            }

            services.AddTransient<EmployerFinanceDbContext, EmployerFinanceDbContext>(provider => provider.GetService<EmployerFinanceDbContext>());
            services.AddTransient(provider => new Lazy<EmployerFinanceDbContext>(provider.GetService<EmployerFinanceDbContext>()));

            return services;
        }
    }
}
