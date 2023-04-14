using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using NServiceBus.Persistence;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.NServiceBus.SqlServer.Data;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class EntityFrameworkStartup
{
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, EmployerFinanceConfiguration config)
    {
        return services.AddScoped(p =>
        {
            var unitOfWorkContext = p.GetService<IUnitOfWorkContext>();
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            EmployerFinanceDbContext dbContext;
            try
            {                    
                var synchronizedStorageSession = unitOfWorkContext.Get<SynchronizedStorageSession>();
                var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();
                var optionsBuilder = new DbContextOptionsBuilder<EmployerFinanceDbContext>().UseSqlServer(sqlStorageSession.Connection);                    
                dbContext = new EmployerFinanceDbContext(sqlStorageSession.Connection, config, optionsBuilder.Options, azureServiceTokenProvider);
                dbContext.Database.UseTransaction(sqlStorageSession.Transaction);
            }
            catch (KeyNotFoundException)
            {
                var connection = DatabaseExtensions.GetSqlConnection(config.DatabaseConnectionString);
                var optionsBuilder = new DbContextOptionsBuilder<EmployerFinanceDbContext>().UseSqlServer(connection);
                dbContext = new EmployerFinanceDbContext(optionsBuilder.Options);
            }

            return dbContext;
        });
        
    }
}