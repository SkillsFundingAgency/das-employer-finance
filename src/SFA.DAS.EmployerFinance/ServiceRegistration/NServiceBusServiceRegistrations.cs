using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using Endpoint = NServiceBus.Endpoint;

namespace SFA.DAS.EmployerFinance.ServiceRegistration
{
    public static class NServiceBusServiceRegistrations
    {
        private const string EndPointName = "SFA.DAS.EmployerFinance";

        public static void StartNServiceBus(this UpdateableServiceProvider services, IConfiguration configuaration, bool isDevOrLocal)
        {
            var employerFinanceConfiguaration = configuaration.Get<EmployerFinanceConfiguration>();

            var databaseConnectionString = employerFinanceConfiguaration.DatabaseConnectionString;

            if(string.IsNullOrWhiteSpace(databaseConnectionString))
            {
                throw new Exception("DatabaseConnectionString configuration value is empty.");
            }

            var endpointConfiguration = new EndpointConfiguration(EndPointName)
            .UseErrorQueue($"{EndPointName}-errors")
            .UseInstallers()
            .UseMessageConventions()
            .UseServicesBuilder(services)
            .UseNewtonsoftJsonSerializer()
            //.UseNLogFactory()
            .UseOutbox(true)
            .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(databaseConnectionString))
            //.UseUnitOfWork()
                ;

            if (isDevOrLocal)
            {
                endpointConfiguration.UseLearningTransport();
            }
            else
            {
                //TODO MAC-192
                //endpointConfiguration.UseAzureServiceBusTransport(employerFinanceConfiguaration.MessageServiceBusConnectionString, r => { });
            }

            if (!string.IsNullOrEmpty(employerFinanceConfiguaration.NServiceBusLicense))
            {
                endpointConfiguration.License(employerFinanceConfiguaration.NServiceBusLicense);
            }

            var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            services.AddSingleton(p => endpoint)
                .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}
