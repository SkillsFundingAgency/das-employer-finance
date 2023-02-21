using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;
using SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.AppStart;

public class WhenAddingServicesToTheContainer
{
    [TestCase(typeof(EmployerAccountTransactionsOrchestrator))]
    [TestCase(typeof(HomeOrchestrator))]
    [TestCase(typeof(TransfersOrchestrator))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Orchestrators(Type toResolve)
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        Assert.IsNotNull(type);
    }
    [TestCase(typeof(IEmployerAccountAuthorisationHandler))]
    [TestCase(typeof(ICustomClaims))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Services(Type toResolve)
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        Assert.IsNotNull(type);
    }
    
    [TestCase(typeof(IRequestHandler<GetTransferRequestsQuery, GetTransferRequestsResponse>))]
    [TestCase(typeof(IRequestHandler<FindAccountCoursePaymentsQuery, FindAccountCoursePaymentsResponse>))]
    [TestCase(typeof(IRequestHandler<FindAccountProviderPaymentsQuery, FindAccountProviderPaymentsResponse>))]
    [TestCase(typeof(IRequestHandler<GetEmployerAccountTransactionsQuery, GetEmployerAccountTransactionsResponse>))]
    [TestCase(typeof(IRequestHandler<FindEmployerAccountLevyDeclarationTransactionsQuery, FindEmployerAccountLevyDeclarationTransactionsResponse>))]
    [TestCase(typeof(IRequestHandler<GetPayeSchemeByRefQuery, GetPayeSchemeByRefResponse>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Handlers(Type toResolve)
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        Assert.IsNotNull(type);
    }
    
    [Test]
    public void Then_Resolves_Authorization_Handlers()
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();
            
        var type = provider.GetServices(typeof(IAuthorizationHandler)).ToList();
            
        Assert.IsNotNull(type);
        type.Count.Should().Be(3);
        type.Should().ContainSingle(c => c.GetType() == typeof(EmployerAccountAllRolesAuthorizationHandler));
        type.Should().ContainSingle(c => c.GetType() == typeof(EmployerAccountOwnerAuthorizationHandler));
    }


    private static void SetupServiceCollection(ServiceCollection serviceCollection)
    {
        var configuration = GenerateConfiguration();
        var financeConfiguration = configuration
            .GetSection("EmployerFinanceConfiguration")
            .Get<EmployerFinanceConfiguration>();
        serviceCollection.AddSingleton(Mock.Of<IWebHostEnvironment>());
        serviceCollection.AddSingleton(Mock.Of<IConfiguration>());
        serviceCollection.AddConfigurationOptions(configuration);
        serviceCollection.AddDistributedMemoryCache();
        serviceCollection.AddApplicationServices(configuration);
        serviceCollection.AddAuthenticationServices();
        serviceCollection.AddDatabaseRegistration(financeConfiguration.DatabaseConnectionString);
        serviceCollection.AddDataRepositories();
        serviceCollection.AddOrchestrators();
        serviceCollection.AddLogging();
    }
    
    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("EmployerFinanceConfiguration:CommitmentsApiV2ClientConfiguration:ApiBaseUrl", "https://test1.com/"),
                new KeyValuePair<string, string>("EmployerFinanceConfiguration:EmployerFinanceOuterApiConfiguration:BaseUrl", "https://test.com/"),
                new KeyValuePair<string, string>("EmployerFinanceConfiguration:EmployerFinanceOuterApiConfiguration:Key", "123edc"),
                new KeyValuePair<string, string>("EmployerFinanceConfiguration:DatabaseConnectionString", "Data Source=.;Initial Catalog=SFA.DAS.EmployerFinance;Integrated Security=True;Pooling=False;Connect Timeout=30"),
                new KeyValuePair<string, string>("EnvironmentName", "test"),
                new KeyValuePair<string, string>("SFA.DAS.Encoding", "{'Encodings':[{'EncodingType':'AccountId','Salt':'test','MinHashLength':6,'Alphabet':'46789BCDFGHJKLMNPRSTVWXY'}]}")
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}