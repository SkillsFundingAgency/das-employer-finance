using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Extensions;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class DatabaseServiceRegistrations
{
    public static IServiceCollection AddDatabaseRegistration(this IServiceCollection services, string databaseConnectionString)
    {
        var dbConnection = DatabaseExtensions.GetSqlConnection(databaseConnectionString);

        services.AddDbContext<EmployerFinanceDbContext>(options => options.UseSqlServer(dbConnection), ServiceLifetime.Transient);

        services.AddTransient(provider => new Lazy<EmployerFinanceDbContext>(provider.GetService<EmployerFinanceDbContext>()));

        return services;
    }
}