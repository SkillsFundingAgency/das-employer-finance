using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.Memory;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.ServiceRegistrations;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Queries.GetAccountBalances;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndPeriod;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.Queries.GetTotalPayments;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
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

    [TestCase(typeof(IRequestHandler<GetEmployerAccountTransactionsQuery, GetEmployerAccountTransactionsResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountTransactionSummaryRequest, GetAccountTransactionSummaryResponse>))]
    [TestCase(typeof(IRequestHandler<GetLevyDeclarationRequest, GetLevyDeclarationResponse>))]
    [TestCase(typeof(IRequestHandler<GetLevyDeclarationsByAccountAndPeriodRequest, GetLevyDeclarationsByAccountAndPeriodResponse>))]
    [TestCase(typeof(IRequestHandler<GetEnglishFractionHistoryQuery, GetEnglishFractionHistoryResposne>))]
    [TestCase(typeof(IRequestHandler<GetEnglishFractionCurrentQuery, GetEnglishFractionCurrentResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountBalancesRequest, GetAccountBalancesResponse>))]
    [TestCase(typeof(IRequestHandler<GetTransferAllowanceQuery, GetTransferAllowanceResponse>))]
    [TestCase(typeof(IRequestHandler<GetTotalPaymentsQuery, GetTotalPaymentsResponse>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Query_Handlers(Type toResolve)
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
            .GetSection("EmployerFinanceConfiguration")
            .Get<EmployerFinanceConfiguration>();

        services.AddSingleton(Mock.Of<IWebHostEnvironment>());
        services.AddSingleton(Mock.Of<IConfiguration>());
        services.AddApiConfigurationSections(configuration);
        services.AddDistributedMemoryCache();
        services.AddApplicationServices();
        //services.AddAuthenticationServices();
        services.AddDatabaseRegistration();
        services.AddDataRepositories();
        services.AddOrchestrators();
        services.AddLogging();
        services.AddHmrcServices();
        services.AddMediatR(typeof(GetPayeSchemeByRefQuery));
        services.AddMediatorValidators();
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