using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using StructureMap;

namespace SFA.DAS.EmployerFinance.Api.DependencyResolution;

public class DefaultRegistry : Registry
{
    private const string AzureResource = "https://database.windows.net/";

    public DefaultRegistry()
    {
        Scan(s =>
        {
            s.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
            s.RegisterConcreteTypesAgainstTheFirstInterface();
        });

        For<EmployerFinanceDbContext>().Use(c => GetFinanceDbContext(c));
    }

    private static EmployerFinanceDbContext GetFinanceDbContext(IContext context)
    {
        var connectionString = GetEmployerFinanceConnectionString(context);
        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        var useManagedIdentity = !connectionStringBuilder.IntegratedSecurity && string.IsNullOrEmpty(connectionStringBuilder.UserID);

        var optionsBuilder = new DbContextOptionsBuilder<EmployerFinanceDbContext>();
        if (useManagedIdentity)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result;
            var sqlConnection = new SqlConnection
            {
                ConnectionString = connectionString,
                AccessToken = accessToken,
            };
            optionsBuilder.UseSqlServer(sqlConnection);
        }

        else
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        return new EmployerFinanceDbContext(optionsBuilder.Options);
    }

    private static string GetEmployerFinanceConnectionString(IContext context)
    {
        return context.GetInstance<EmployerFinanceConfiguration>().DatabaseConnectionString;
    }
}