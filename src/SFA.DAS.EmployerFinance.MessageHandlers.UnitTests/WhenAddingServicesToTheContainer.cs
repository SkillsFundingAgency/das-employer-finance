using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;
using SFA.DAS.EmployerFinance.MessageHandlers.Extensions;
using SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests;

public class WhenAddingServicesToTheContainer
{

    [TestCase(typeof(IHandleMessages<CreateAccountPayeCommand>))]
    [TestCase(typeof(IHandleMessages<DraftExpireAccountFundsCommand>))]
    [TestCase(typeof(IHandleMessages<DraftExpireFundsCommand>))]
    [TestCase(typeof(IHandleMessages<ExpireAccountFundsCommand>))]
    [TestCase(typeof(IHandleMessages<ImportAccountLevyDeclarationsCommand>))]
    [TestCase(typeof(IHandleMessages<ImportAccountPaymentsCommand>))]
    [TestCase(typeof(IHandleMessages<ImportPaymentsCommand>))]
    [TestCase(typeof(IHandleMessages<ProcessPeriodEndPaymentsCommand>))]
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

        services.AddSingleton<IConfiguration>(configuration);
        services.AddConfigurationSections(configuration);
        services.AddClientRegistrations();
        services.AddNServiceBus();
        services.AddDataRepositories();
        services.AddApplicationServices();
        services.AddDatabaseRegistration(financeConfiguration.DatabaseConnectionString);
        services.AddUnitOfWork();
        services.AddMediatR(typeof(Program));
        services.AddLogging(_ => { });

        services.AddTransient<IHandleMessages<CreateAccountPayeCommand>, CreateAccountPayeCommandHandler>();
        services.AddTransient<IHandleMessages<DraftExpireAccountFundsCommand>, DraftExpireAccountFundsCommandHandler>();
        services.AddTransient<IHandleMessages<DraftExpireFundsCommand>, DraftExpireFundsCommandHandler>();
        services.AddTransient<IHandleMessages<ExpireAccountFundsCommand>, ExpireAccountFundsCommandHandler>();
        services.AddTransient<IHandleMessages<ImportAccountLevyDeclarationsCommand>, ImportAccountLevyDeclarationsCommandHandler>();
        services.AddTransient<IHandleMessages<ImportAccountPaymentsCommand>, ImportAccountPaymentsCommandHandler>();
        services.AddTransient<IHandleMessages<ImportPaymentsCommand>, ImportPaymentsCommandHandler>();
        services.AddTransient<IHandleMessages<ProcessPeriodEndPaymentsCommand>, ProcessPeriodEndPaymentsCommandHandler>();
        services.AddTransient<IHandleMessages<ImportPaymentsCommand>, ImportPaymentsCommandHandler>();
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
                new("EnvironmentName", "test"),
                new("DeclarationsEnabled", "true"),
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}