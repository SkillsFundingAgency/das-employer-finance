using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Extensions;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class DatabaseServiceRegistrations
{
    public static IServiceCollection AddDatabaseRegistration(this IServiceCollection services, string databaseConnectionString)
    {
        ArgumentNullException.ThrowIfNull(databaseConnectionString);

        services.AddDbContext<EmployerFinanceDbContext>(options => options.UseSqlServer(DatabaseExtensions.GetSqlConnection(databaseConnectionString)), ServiceLifetime.Transient);

        services.AddTransient(provider => new Lazy<EmployerFinanceDbContext>(provider.GetService<EmployerFinanceDbContext>()));

        return services;
    }
}