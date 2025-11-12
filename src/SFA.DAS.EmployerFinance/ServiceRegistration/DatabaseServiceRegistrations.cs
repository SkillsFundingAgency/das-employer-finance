using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Infrastructure;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class DatabaseServiceRegistrations
{
    public static IServiceCollection AddDatabaseRegistration(this IServiceCollection services)
    {
        services.TryAddScoped<ISqlConnectionFactory, ManagedIdentitySqlConnectionFactory>();

        services.AddDbContext<EmployerFinanceDbContext>((sp, options) =>
        {
            var connectionFactory = sp.GetRequiredService<ISqlConnectionFactory>();
            var connectionString = sp.GetRequiredService<EmployerFinanceConfiguration>().DatabaseConnectionString;
            var dbConnection = connectionFactory.Create(connectionString);
            options.UseSqlServer(dbConnection);
        }, ServiceLifetime.Transient);

        services.AddScoped(provider => new Lazy<EmployerFinanceDbContext>(provider.GetService<EmployerFinanceDbContext>()));
        
        return services;
    }
}