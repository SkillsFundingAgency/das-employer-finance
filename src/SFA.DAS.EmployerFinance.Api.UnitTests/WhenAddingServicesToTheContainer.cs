using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.ServiceRegistrations;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.ServiceRegistration;

namespace SFA.DAS.EmployerFinance.Api.UnitTests;

public class WhenAddingServicesToTheContainer
{
    [TestCase(typeof(AccountTransactionsOrchestrator))]
    [TestCase(typeof(FinanceOrchestrator))]
    [TestCase(typeof(StatisticsOrchestrator))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Orchestrators(Type toResolve)
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        Assert.IsNotNull(type);
    }

    //[TestCase(typeof(IEmployerAccountAuthorisationHandler))]
    //[TestCase(typeof(ICustomClaims))]
    //public void Then_The_Dependencies_Are_Correctly_Resolved_For_Services(Type toResolve)
    //{
    //    var services = new ServiceCollection();
    //    SetupServiceCollection(services);
    //    var provider = services.BuildServiceProvider();

    //    var type = provider.GetService(toResolve);
    //    Assert.IsNotNull(type);
    //}
    
    //[TestCase(typeof(IRequestHandler<GetTransferRequestsQuery, GetTransferRequestsResponse>))]
    //[TestCase(typeof(IRequestHandler<FindAccountCoursePaymentsQuery, FindAccountCoursePaymentsResponse>))]
    //public void Then_The_Dependencies_Are_Correctly_Resolved_For_Handlers(Type toResolve)
    //{
    //    var services = new ServiceCollection();
    //    SetupServiceCollection(services);
    //    var provider = services.BuildServiceProvider();

    //    var type = provider.GetService(toResolve);
    //    Assert.IsNotNull(type);
    //}
    
    //[Test]
    //public void Then_Resolves_Authorization_Handlers()
    //{
    //    var services = new ServiceCollection();
    //    SetupServiceCollection(services);
    //    var provider = services.BuildServiceProvider();
            
    //    var type = provider.GetServices(typeof(IAuthorizationHandler)).ToList();
            
    //    Assert.IsNotNull(type);
    //    type.Count.Should().Be(3);
    //    type.Should().ContainSingle(c => c.GetType() == typeof(EmployerAccountAllRolesAuthorizationHandler));
    //    type.Should().ContainSingle(c => c.GetType() == typeof(EmployerAccountOwnerAuthorizationHandler));
    //}


    private static void SetupServiceCollection(IServiceCollection services)
    {
        var configuration = GenerateConfiguration();
        var financeConfiguration = configuration
            .GetSection("EmployerFinanceConfiguration")
            .Get<EmployerFinanceConfiguration>();

        services.AddSingleton(Mock.Of<IWebHostEnvironment>());
        services.AddSingleton(Mock.Of<IConfiguration>());
        services.AddApiConfigurationSections(configuration);
        services.AddDistributedMemoryCache();
        services.AddApplicationServices();
        //services.AddAuthenticationServices();
        services.AddDatabaseRegistration(financeConfiguration.DatabaseConnectionString);
        services.AddDataRepositories();
        services.AddOrchestrators();
        services.AddLogging();
        services.AddMediatR(typeof(GetPayeSchemeByRefQuery));
        services.AddAutoMapper(typeof(Startup), typeof(GetPayeSchemeByRefHandler));

    }
    
    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new("EmployerFinanceConfiguration:CommitmentsApiV2ClientConfiguration:ApiBaseUrl", "https://test1.com/"),
                new("EmployerFinanceConfiguration:EmployerFinanceOuterApiConfiguration:BaseUrl", "https://test.com/"),
                new("EmployerFinanceConfiguration:EmployerFinanceOuterApiConfiguration:Key", "123edc"),
                new("EmployerFinanceConfiguration:DatabaseConnectionString", "Data Source=.;Initial Catalog=SFA.DAS.EmployerFinance;Integrated Security=True;Pooling=False;Connect Timeout=30"),
                new("EnvironmentName", "test"),
                new("SFA.DAS.Encoding", "{'Encodings':[{'EncodingType':'AccountId','Salt':'test','MinHashLength':6,'Alphabet':'46789BCDFGHJKLMNPRSTVWXY'}]}")
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}