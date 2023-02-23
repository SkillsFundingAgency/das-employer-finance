using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.MessageHandlers.TestHarness.Extensions;
using SFA.DAS.EmployerFinance.MessageHandlers.TestHarness.Scenarios;

namespace SFA.DAS.EmployerFinance.MessageHandlers.TestHarness;

public static class Program
{
    public static async Task Main()
    {
        var provider = RegisterServices();

        await provider.GetService<SendDraftExpireFundsCommand>().Run();
        //await container.GetInstance<PublishCohortCreatedEvents>().Run();
    }

    private static IServiceProvider RegisterServices()
    {
        var configuration = GenerateConfiguration();
        var financeConfiguration = configuration
            .GetSection(ConfigurationKeys.EmployerFinance)
            .Get<EmployerFinanceConfiguration>();

        return new ServiceCollection()
            .AddNServiceBus()
            .AddSingleton<SendDraftExpireFundsCommand>()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(financeConfiguration)
            .BuildServiceProvider();
    }

    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new("SFA.DAS.EmployerFinance:DatabaseConnectionString", "Data Source=.;Initial Catalog=SFA.DAS.EmployerFinance;Integrated Security=True;Pooling=False;Connect Timeout=30"),
                new("SFA.DAS.EmployerFinance:PaymentsEventsApi:ApiBaseUrl", "test"),
                new("SFA.DAS.EmployerFinance:PaymentsEventsApi:IdentifierUri", "test"),
                new("SFA.DAS.EmployerFinance:ServiceBusConnectionString", "test"),
                new("SFA.DAS.EmployerFinance:NServiceBusLicense", "test"),
                new("EnvironmentName", "LOCAL"),
                new("DeclarationsEnabled", "true"),
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}