using HMRC.ESFA.Levy.Api.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.AppStart;

public class WhenAddingServicesToTheContainer
{
    [TestCase(typeof(TransfersOrchestrator))]
    [TestCase(typeof(IEmployerAccountTransactionsOrchestrator))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Orchestrators(Type toResolve)
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        type.Should().NotBeNull();
    }
    
    [TestCase(typeof(IEmployerAccountAuthorisationHandler))]
    [TestCase(typeof(ICommitmentsV2ApiClient))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Services(Type toResolve)
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);
        type.Should().NotBeNull();
    }
    
    [Test]
    public void Then_Resolves_Authorization_Handlers()
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();
            
        var type = provider.GetServices(typeof(IAuthorizationHandler)).ToList();
            
        Assert.Multiple(() =>
        {
            type.Should().NotBeNull();
            type.Count.Should().Be(3);
        
            type.Should().ContainSingle(c => c.GetType() == typeof(EmployerAccountAllRolesAuthorizationHandler));
            type.Should().ContainSingle(c => c.GetType() == typeof(EmployerAccountOwnerAuthorizationHandler));
        });
    }

    private static void SetupServiceCollection(IServiceCollection services)
    {
        var configuration = GenerateConfiguration();

        services.AddSingleton(Mock.Of<IWebHostEnvironment>());
        services.AddSingleton(Mock.Of<IConfiguration>());
        services.AddConfigurationOptions(configuration);
        services.AddDistributedMemoryCache();
        services.AddApplicationServices(configuration);
        services.AddAuthenticationServices();
        services.AddDatabaseRegistration();
        services.AddDataRepositories();
        services.AddOrchestrators();
        services.AddLogging(); 
    }
    
    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new("CommitmentsApiV2ClientConfiguration:ApiBaseUrl", "https://test1.com/"),
                new("AccountApiConfiguration:ApiBaseUrl", "https://test1.com/"),
                new("EmployerFinanceOuterApiConfiguration:BaseUrl", "https://test.com/"),
                new("EmployerFinanceOuterApiConfiguration:Key", "123edc"),
                new("EmployerFinanceConfiguration:DatabaseConnectionString", "Data Source=.;Initial Catalog=SFA.DAS.EmployerFinance;Integrated Security=True;Pooling=False;Connect Timeout=30"),
                new("EnvironmentName", "test"),
                new("SFA.DAS.Encoding", "{'Encodings':[{'EncodingType':'AccountId','Salt':'test','MinHashLength':6,'Alphabet':'46789BCDFGHJKLMNPRSTVWXY'}]}")
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}

public class StubHmrcService: IHmrcService
{
    public Task<EmpRefLevyInformation> GetEmprefInformation(string empRef)
    {
        throw new NotImplementedException();
    }

    public Task<EnglishFractionDeclarations> GetEnglishFractions(string empRef)
    {
        throw new NotImplementedException();
    }

    public Task<EnglishFractionDeclarations> GetEnglishFractions(string empRef, DateTime? fromDate)
    {
        throw new NotImplementedException();
    }

    public Task<DateTime> GetLastEnglishFractionUpdate()
    {
        throw new NotImplementedException();
    }

    public Task<LevyDeclarations> GetLevyDeclarations(string empRef, DateTime? fromDate)
    {
        throw new NotImplementedException();
    }
}