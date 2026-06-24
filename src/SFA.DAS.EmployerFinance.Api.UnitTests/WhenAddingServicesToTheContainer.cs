using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.Memory;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.ServiceRegistrations;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateAuditJob;
using SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;
using SFA.DAS.EmployerFinance.Commands.CreateAuditWorkflowLog;
using SFA.DAS.EmployerFinance.Queries.GetAccountBalances;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobLogs;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobs;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobSummary;
using SFA.DAS.EmployerFinance.Queries.GetAuditWorkflowLogs;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;
using SFA.DAS.EmployerFinance.Queries.GetLastEnglishFractionCalculationDate;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndPeriod;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.Queries.GetTotalPayments;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.EmployerFinance.ServiceRegistration;
 
namespace SFA.DAS.EmployerFinance.Api.UnitTests;
 
public class WhenAddingServicesToTheContainer
{
    [TestCase(typeof(AccountTransactionsOrchestrator))]
    [TestCase(typeof(AuditLogOrchestrator))]
    [TestCase(typeof(FinanceOrchestrator))]
    [TestCase(typeof(LevyDeclarationOrchestrator))]
    [TestCase(typeof(EnglishFractionCalculationDateOrchestrator))]
    [TestCase(typeof(StatisticsOrchestrator))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Orchestrators(Type toResolve)
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();
 
        var type = provider.GetService(toResolve);
        type.Should().NotBeNull();
    }
 
    [TestCase(typeof(IRequestHandler<GetEmployerAccountTransactionsQuery, GetEmployerAccountTransactionsResponse>))]
    [TestCase(typeof(IRequestHandler<CreateAuditJobCommand, CreateAuditJobCommandResult>))]
    [TestCase(typeof(IRequestHandler<CreateAuditWorkflowLogCommand, CreateAuditWorkflowLogCommandResult>))]
    [TestCase(typeof(IRequestHandler<GetAccountTransactionSummaryRequest, GetAccountTransactionSummaryResponse>))]
    [TestCase(typeof(IRequestHandler<GetAuditJobsQuery, GetAuditJobsQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetAuditJobSummaryQuery, GetAuditJobSummaryQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetAuditJobLogsQuery, GetAuditJobLogsQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetAuditWorkflowLogsQuery, GetAuditWorkflowLogsQueryResponse>))]
    [TestCase(typeof(IRequestHandler<GetLevyDeclarationRequest, GetLevyDeclarationResponse>))]
    [TestCase(typeof(IRequestHandler<GetLevyDeclarationSubmissionIdsQuery, List<long>>))]
    [TestCase(typeof(IRequestHandler<GetExistingPeriod12LevyDeclarationsQuery, List<ExistingPeriod12LevyDeclarationResult>>))]
    [TestCase(typeof(IRequestHandler<GetLevyDeclarationsByAccountAndPeriodRequest, GetLevyDeclarationsByAccountAndPeriodResponse>))]
    [TestCase(typeof(IRequestHandler<GetEnglishFractionHistoryQuery, GetEnglishFractionHistoryResposne>))]
    [TestCase(typeof(IRequestHandler<GetEnglishFractionCurrentQuery, GetEnglishFractionCurrentResponse>))]
    [TestCase(typeof(IRequestHandler<GetLastEnglishFractionCalculationDateQuery, GetLastEnglishFractionCalculationDateResponse>))]
    [TestCase(typeof(IRequestHandler<GetAccountBalancesRequest, GetAccountBalancesResponse>))]
    [TestCase(typeof(IRequestHandler<GetTransferAllowanceQuery, GetTransferAllowanceResponse>))]
    [TestCase(typeof(IRequestHandler<GetTotalPaymentsQuery, GetTotalPaymentsResponse>))]
    [TestCase(typeof(IRequestHandler<PersistLevyDeclarationsCommand, PersistLevyDeclarationsResponse>))]
    public void Then_The_Dependencies_Are_Correctly_Resolved_For_Query_Handlers(Type toResolve)
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
 
        services.AddSingleton(Mock.Of<IWebHostEnvironment>());
        services.AddSingleton(Mock.Of<IConfiguration>());
        services.AddApiConfigurationSections(configuration);
        services.AddDistributedMemoryCache();
        services.AddApplicationServices();
        services.AddRouting();
        services.AddDatabaseRegistration();
        services.AddDataRepositories();
        services.AddOrchestrators();
        services.AddLogging();
        services.AddHmrcServices();
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(GetPayeSchemeByRefQuery).Assembly));
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
                new("EmployerFinanceConfiguration:SqlConnectionString", "Data Source=.;Initial Catalog=SFA.DAS.EmployerFinance;Integrated Security=True;Pooling=False;Connect Timeout=30"),
                new("EnvironmentName", "test"),
                new("SFA.DAS.Encoding", "{'Encodings':[{'EncodingType':'AccountId','Salt':'test','MinHashLength':6,'Alphabet':'46789BCDFGHJKLMNPRSTVWXY'}]}")
            }
        };
 
        var provider = new MemoryConfigurationProvider(configSource);
 
        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}
 