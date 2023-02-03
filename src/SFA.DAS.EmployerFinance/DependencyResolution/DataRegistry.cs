using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using NServiceBus.Persistence;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.MarkerInterfaces;
using SFA.DAS.HashingService;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.UnitOfWork.Context;
using System.Data.Common;
using System;
using StructureMap;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.EmployerFinance.DependencyResolution
{
    public class DataRegistry : Registry
    {
        private const string AzureResource = "https://database.windows.net/";

        public DataRegistry()
        {
            var environmentName = ConfigurationManager.AppSettings["EnvironmentName"];

            For<DbConnection>().Use($"Build DbConnection", c =>
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                return environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase)
                    ? new SqlConnection(GetEmployerFinanceConnectionString(c))
                    : new SqlConnection
                    {
                        ConnectionString = GetEmployerFinanceConnectionString(c),
                        AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result
                    };
            });

            For<EmployerFinanceDbContext>().Use(c => GetDbContext(c));
        }

        private EmployerFinanceDbContext GetDbContext(IContext context)
        {
            var unitOfWorkContext = context.GetInstance<IUnitOfWorkContext>();

            var optionsBuilder = new DbContextOptionsBuilder<EmployerFinanceDbContext>();
            var connectionString = GetEmployerFinanceConnectionString(context);
            optionsBuilder.UseSqlServer(connectionString);
            return new EmployerFinanceDbContext(optionsBuilder.Options);
        }

        private string GetEmployerFinanceConnectionString(IContext context)
        {
            return context.GetInstance<EmployerFinanceConfiguration>().DatabaseConnectionString;
        }

    }
}
