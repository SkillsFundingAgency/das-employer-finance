using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Extensions;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class DatabaseServiceRegistrations
{
    public static IServiceCollection AddDatabaseRegistration(this IServiceCollection services)
    {
        services.AddDbContext<EmployerFinanceDbContext>((sp, options) =>
        {
            var dbConnection = DatabaseExtensions.GetSqlConnection(sp.GetService<EmployerFinanceConfiguration>().DatabaseConnectionString);
            options.UseSqlServer(dbConnection);
        }, ServiceLifetime.Transient);

        services.AddTransient(provider => new Lazy<EmployerFinanceDbContext>(provider.GetService<EmployerFinanceDbContext>()));
        services.AddScoped(provider => provider.GetService<EmployerFinanceDbContext>());

        return services;
    }
}