﻿using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using Endpoint = NServiceBus.Endpoint;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class NServiceBusServiceRegistrations
{
    private const string EndPointName = "SFA.DAS.EmployerFinance.Web";
    
    public static void StartNServiceBus(this UpdateableServiceProvider services, IConfiguration configuration, bool isDevOrLocal)
    {
        var employerFinanceConfiguration = configuration.GetSection(nameof(EmployerFinanceConfiguration)).Get<EmployerFinanceConfiguration>();

        var databaseConnectionString = employerFinanceConfiguration.DatabaseConnectionString;

        if (string.IsNullOrWhiteSpace(databaseConnectionString))
        {
            throw new Exception("DatabaseConnectionString configuration value is empty.");
        }
        
        var endpointConfiguration = new EndpointConfiguration(EndPointName)
            .UseErrorQueue($"{EndPointName}-errors")
            .UseInstallers()
            .UseNewMessageConventions()
            .UseServicesBuilder(services)
            .UseNewtonsoftJsonSerializer()
            .UseOutbox(true)
            .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(databaseConnectionString))
            .UseUnitOfWork();

        if (isDevOrLocal)
        {
            endpointConfiguration.UseLearningTransport();
        }
        else
        {
            endpointConfiguration.UseAzureServiceBusTransport(employerFinanceConfiguration.ServiceBusConnectionString, r => { });
        }

        if (!string.IsNullOrEmpty(employerFinanceConfiguration.NServiceBusLicense))
        {
            endpointConfiguration.License(employerFinanceConfiguration.NServiceBusLicense);
        }

        var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

        services.AddSingleton(p => endpoint)
            .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
            .AddHostedService<NServiceBusHostedService>();
    }
}