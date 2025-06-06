using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Formatters;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads.Csv;
using SFA.DAS.EmployerFinance.Formatters.TransactionDownloads.Excel;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class ApplicationServiceRegistrations
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
        services.AddDateTimeServices(configuration);
        services.AddAutoMapper(typeof(Startup).Assembly);
        services.AddAutoMapper(typeof(AccountMappings).Assembly);
        services.AddMediatR(x=> x.RegisterServicesFromAssembly(typeof(GetTransferRequestsQuery).Assembly));
        services.AddWebMediatorValidators();
        services.AddHttpContextAccessor();

        services.AddScoped<IProviderService, ProviderServiceCache>();
        services.AddScoped<IProviderService, ProviderServiceFromDb>();

        services.AddHttpClient<IOuterApiClient, OuterApiClient>();
        services.AddHttpClient<ICommitmentsV2ApiClient, CommitmentsV2ApiClient>();

        services.AddTransient<IDasAccountService, DasAccountService>();
        services.AddTransient<IDasForecastingService, DasForecastingService>();
        services.AddTransient<IDasLevyService, DasLevyService>();

        //TODO MAC-192 - was services.Decorate
        services.AddTransient<IApprenticeshipInfoServiceWrapper, ApprenticeshipInfoServiceWrapper>();

        services.AddScoped<IAccountApiClient, AccountApiClient>();
        services.AddTransient<IExcelService, ExcelService>();

        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddTransient<IUserAccountRepository, UserAccountRepository>();

        services.AddScoped(typeof(ICookieService<>), typeof(HttpCookieService<>));
        services.AddScoped(typeof(ICookieStorageService<>), typeof(CookieStorageService<>));
        services.AddScoped<IUrlActionHelper, UrlActionHelper>();

        services.AddTransient<IHmrcDateService, HmrcDateService>();
        
        services.AddTransient<IPaymentService, PaymentService>();
        services.AddTransient<ITransfersService, TransfersService>();

        services.AddTransient<ITokenServiceApiClient, TokenServiceApiClient>();

        services.AddTransient<IEncodingService, EncodingService>();

        services.AddTransient<ITransactionFormatterFactory, TransactionFormatterFactory>();

        services.AddTransient<ITransactionFormatter, LevyCsvTransactionFormatter>();
        services.AddTransient<ITransactionFormatter, NonLevyCsvTransactionFormatter>();
        services.AddTransient<ITransactionFormatter, LevyExcelTransactionFormatter>();
        services.AddTransient<ITransactionFormatter, NonLevyExcelTransactionFormatter>();
        
        services.AddTransient<IAccountClaimsService, AccountClaimsService>();
        services.AddTransient<IHmrcService, HmrcService>();
    }
}
