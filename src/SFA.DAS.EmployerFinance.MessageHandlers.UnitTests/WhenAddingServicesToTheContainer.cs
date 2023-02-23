using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.MessageHandlers.Extensions;
using SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests;

public class WhenAddingServicesToTheContainer
{

    [TestCase(typeof(IHandleMessages<ImportPaymentsCommand>))]
    [TestCase(typeof(IHandleMessages<CreateAccountPayeCommand>))]

    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Command_Handlers(Type toResolve)
    {
        var services = new ServiceCollection();
        SetupServiceCollection(services);
        var provider = services.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        Assert.IsNotNull(type);
    }

    private static void SetupServiceCollection(IServiceCollection services)
    {
        var configuration = GenerateConfiguration();
        var financeConfiguration = configuration
            .GetSection(ConfigurationKeys.EmployerFinance)
            .Get<EmployerFinanceConfiguration>();

        services.AddSingleton(Mock.Of<IWebHostEnvironment>());
        services.AddSingleton(Mock.Of<IConfiguration>());
        services.AddConfigurationSections(configuration);
        services.AddClientRegistrations();
        services.AddNServiceBus();
        services.AddDataRepositories();
        services.AddDatabaseRegistration(financeConfiguration.DatabaseConnectionString);
        services.AddUnitOfWork();
        services.AddMediatR(typeof(Program));
        services.AddLogging(c => { });

        services.AddTransient<IHandleMessages<ImportPaymentsCommand>, ImportPaymentsCommandHandler>();
        services.AddTransient<IHandleMessages<CreateAccountPayeCommand>, CreateAccountPayeCommandHandler>();
    }


    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new("SFA.DAS.EmployerFinance:CommitmentsApiV2ClientConfiguration:ApiBaseUrl", "https://test1.com/"),
                new("SFA.DAS.EmployerFinance:EmployerFinanceOuterApiConfiguration:BaseUrl", "https://test.com/"),
                new("SFA.DAS.EmployerFinance:EmployerFinanceOuterApiConfiguration:Key", "123edc"),
                new("SFA.DAS.EmployerFinance:DatabaseConnectionString", "Data Source=.;Initial Catalog=SFA.DAS.EmployerFinance;Integrated Security=True;Pooling=False;Connect Timeout=30"),
                new("SFA.DAS.EmployerFinance:PaymentsEventsApi:ApiBaseUrl", "test"),
                new("SFA.DAS.EmployerFinance:PaymentsEventsApi:IdentifierUri", "test"),
                new("EnvironmentName", "test"),
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}