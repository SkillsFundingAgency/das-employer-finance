using System;
using System.Configuration;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using StructureMap;

namespace SFA.DAS.EmployerFinance.Jobs.DependencyResolution
{
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

            For<ILoggerFactory>().Use(() => new LoggerFactory().AddNLog()).Singleton();
            For<ILogger>().Use(c => c.GetInstance<ILoggerFactory>().CreateLogger(c.ParentType));
            For<EmployerFinanceDbContext>().Use(c => GetDbContext(c));
            //For<IRunOnceJobsService>().Use<RunOnceJobsService>();
        }

        private static EmployerFinanceDbContext GetDbContext(IContext context)
        {
            var environmentName = ConfigurationManager.AppSettings["EnvironmentName"];

            var connectionString = GetConnectionString(context);
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            bool useManagedIdentity = !connectionStringBuilder.IntegratedSecurity && string.IsNullOrEmpty(connectionStringBuilder.UserID);

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

        private static string GetConnectionString(IContext context)
        {
            return context.GetInstance<EmployerFinanceConfiguration>().DatabaseConnectionString;
        }
    }
}