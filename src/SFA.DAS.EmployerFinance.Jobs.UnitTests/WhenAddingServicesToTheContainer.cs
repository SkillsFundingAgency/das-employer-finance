using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Jobs.Extensions;
using SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;
using SFA.DAS.EmployerFinance.Jobs.ServiceRegistrations;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;

namespace SFA.DAS.EmployerFinance.Jobs.UnitTests;

public class WhenAddingServicesToTheContainer
{
    [TestCase(typeof(ExpireFundsJob))]
    [TestCase(typeof(ImportLevyDeclarationsJob))]
    [TestCase(typeof(ImportPaymentsJob))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Jobs(Type toResolve)
    {
        var services = new ServiceCollection();
        SetupServiceCollection(services);
        var provider = services.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        type.Should().NotBeNull();
    }

    private static void SetupServiceCollection(IServiceCollection services)
    {
        var configuration = GenerateConfiguration();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddConfigurationSections(configuration);
        services.AddNServiceBus();
        services.AddDataRepositories();
        services.AddDatabaseRegistration();
        services.AddUnitOfWork();
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddLogging(_ => { });
        services.AddApplicationServices();
        services.AddTransient<ExpireFundsJob>();
        services.AddTransient<ImportLevyDeclarationsJob>();
        services.AddTransient<ImportPaymentsJob>();
    }


    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new("EmployerFinanceJobsConfiguration:SqlConnectionString", "Data Source=.;Initial Catalog=SFA.DAS.EmployerFinance;Integrated Security=True;Pooling=False;Connect Timeout=30"),
                new("PaymentsEventsApi:ApiBaseUrl", "test"),
                new("PaymentsEventsApi:IdentifierUri", "test"),
                new("EmployerFinanceJobsConfiguration:ServiceBusConnectionString", "test"),
                new("EmployerFinanceJobsConfiguration:NServiceBusLicense", "test"),
                new("EnvironmentName", "LOCAL"),
                new("DeclarationsEnabled", "true"),
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}