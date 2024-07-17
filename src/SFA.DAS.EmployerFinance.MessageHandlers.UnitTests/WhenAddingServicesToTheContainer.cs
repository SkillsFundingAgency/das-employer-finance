using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerFinance.Commands.CreateAccount;
using SFA.DAS.EmployerFinance.Commands.CreateAccountLegalEntity;
using SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;
using SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;
using SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions;
using SFA.DAS.EmployerFinance.Commands.LegalEntitySignAgreement;
using SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;
using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;
using SFA.DAS.EmployerFinance.Commands.RemoveAccountLegalEntity;
using SFA.DAS.EmployerFinance.Commands.RemoveAccountPaye;
using SFA.DAS.EmployerFinance.Commands.RenameAccount;
using SFA.DAS.EmployerFinance.Commands.UpdateEnglishFractions;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;
using SFA.DAS.EmployerFinance.MessageHandlers.Extensions;
using SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Queries.GetAllEmployerAccounts;
using SFA.DAS.EmployerFinance.Queries.GetHMRCLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;
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
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Command_MessageHandlers(Type toResolve)
    {
        var services = new ServiceCollection();
        SetupServiceCollection(services);
        var provider = services.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        type.Should().NotBeNull();
    }

    [TestCase(typeof(IRequestHandler<RefreshEmployerLevyDataCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<GetHMRCLevyDeclarationQuery, GetHMRCLevyDeclarationResponse>))]
    [TestCase(typeof(IRequestHandler<UpdateEnglishFractionsCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<CreateEnglishFractionCalculationDateCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<RefreshPaymentDataCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<RefreshAccountTransfersCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<CreateTransferTransactionsCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<GetPeriodEndsRequest, GetPeriodEndsResponse>))]
    [TestCase(typeof(IRequestHandler<CreateNewPeriodEndCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<GetAllEmployerAccountsRequest, GetAllEmployerAccountsResponse>))]
    [TestCase(typeof(IRequestHandler<CreateAccountLegalEntityCommand, Unit>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Command_MediatorHandlers(Type toResolve)
    {
        var services = new ServiceCollection();
        SetupServiceCollection(services);
        var provider = services.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        type.Should().NotBeNull();
    }

    [TestCase(typeof(IHandleMessages<AddedLegalEntityEvent>))]
    [TestCase(typeof(IHandleMessages<AddedPayeSchemeEvent>))]
    [TestCase(typeof(IHandleMessages<ChangedAccountNameEvent>))]
    [TestCase(typeof(IHandleMessages<CreatedAccountEvent>))]
    [TestCase(typeof(IHandleMessages<DeletedPayeSchemeEvent>))]
    [TestCase(typeof(IHandleMessages<RemovedLegalEntityEvent>))]
    [TestCase(typeof(IHandleMessages<SignedAgreementEvent>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Event_MessageHandlers(Type toResolve)
    {
        var services = new ServiceCollection();
        SetupServiceCollection(services);
        var provider = services.BuildServiceProvider();
        
        var type = provider.GetService(toResolve);
        type.Should().NotBeNull();
    }
    
    [TestCase(typeof(ICommitmentsV2ApiClient))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Clients(Type toResolve)
    {
        var services = new ServiceCollection();
        SetupServiceCollection(services);
        var provider = services.BuildServiceProvider();
        
        var type = provider.GetService(toResolve);
        type.Should().NotBeNull();
    }

    [TestCase(typeof(IRequestHandler<CreateAccountLegalEntityCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<RenameAccountCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<CreateAccountCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<RemoveAccountPayeCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<RemoveAccountLegalEntityCommand, Unit>))]
    [TestCase(typeof(IRequestHandler<LegalEntitySignAgreementCommand, Unit>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Event_MediatorHandlers(Type toResolve)
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
        services.AddClientRegistrations();
        services.AddNServiceBus();
        services.AddDataRepositories();
        services.AddApplicationServices();
        services.AddDatabaseRegistration();
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(RenameAccountCommand).Assembly));
        services.AddAutoMapper(typeof(TransactionRepository));
        services.AddUnitOfWork();
        services.AddMediatorValidators();
        services.AddLogging(_ => { });
        services.AddHmrcServices();
        services.AddProviderServices();
        services.AddCachesRegistrations(true);
        services.AddEmployerFinanceOuterApi();

        RegisterEventHandlers(services);
    }

    private static void RegisterEventHandlers(IServiceCollection services)
    {
        var handlersAssembly = typeof(CreateAccountPayeCommandHandler).Assembly;
        var handlerTypes = handlersAssembly
            .GetTypes()
            .Where(x => x.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>)));

        foreach (var handlerType in handlerTypes)
        {
            var handlerInterface = handlerType.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>));
            services.AddTransient(handlerInterface, handlerType);
        }
    }


    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new("EmployerFinanceJobsConfiguration:ServiceBusConnectionString", "Endpoint=sb://endpoint.servicebus.windows.net/;SharedAccessKeyName=<access-key-name>;SharedAccessKey=<access-key>"),
                new("EmployerFinanceJobsConfiguration:DatabaseConnectionString", "Data Source=.;Initial Catalog=SFA.DAS.EmployerFinance;Integrated Security=True;Pooling=False;Connect Timeout=30"),
                new("PaymentsEventsApi:ApiBaseUrl", "test"),
                new("PaymentsEventsApi:IdentifierUri", "test"),
                new("CommitmentsApiV2ClientConfiguration:ApiBaseUrl", "test"),
                new("CommitmentsApiV2ClientConfiguration:IdentifierUri", "test"),
                new("Hmrc:BaseUrl", "http://test"),
                new("EmployerFinanceOuterApiConfiguration:BaseUrl", "http://test"),
                new("EmployerFinanceOuterApiConfiguration:Key", "test"),
                new("EnvironmentName", "test"),
                new("DeclarationsEnabled", "true"),
                new("SFA.DAS.Encoding", "{\"Encodings\": [{\"EncodingType\": \"AccountId\",\"Salt\": \"and vinegar\",\"MinHashLength\": 32,\"Alphabet\": \"46789BCDFGHJKLMNPRSTVWXY\"}]}"),
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}