using System.Net.Http;
using HMRC.ESFA.Levy.Api.Client;
using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Factories;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Policies.Hmrc;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Time;
using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.Encoding;
using SFA.DAS.Http;
using SFA.DAS.NLog.Logger.Web.MessageHandlers;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.UnitOfWork.NServiceBus.Services;

namespace SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;

public static class ApplicationServiceRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<ICurrentDateTime, CurrentDateTime>();
        services.AddTransient<IExpiredFunds, ExpiredFunds>();
        services.AddTransient<IDasAccountService, DasAccountService>();
        services.AddTransient<IPaymentService, PaymentService>();
        services.AddTransient<ExecutionPolicy, HmrcExecutionPolicy>();
        services.AddTransient<IApprenticeshipInfoServiceWrapper, ApprenticeshipInfoServiceWrapper>();
        services.AddTransient<IOuterApiClient, OuterApiClient>();
        services.AddTransient<ILevyEventFactory, LevyEventFactory>();
        services.AddTransient<IGenericEventFactory, GenericEventFactory>();
        services.AddSingleton<IEncodingService, EncodingService>();
        services.AddSingleton<ILevyImportCleanerStrategy, LevyImportCleanerStrategy>();
        services.AddSingleton<IEventPublisher, EventPublisher>();


        services.AddTransient<ICommitmentsV2ApiClient>(provider =>
        {
            var configuration = provider.GetService<IConfiguration>();
            var commitmentsConfiguration = provider.GetService<CommitmentsApiV2ClientConfiguration>();
            var logger = provider.GetService<ILogger<CommitmentsV2ApiClient>>();
            return new CommitmentsV2ApiClient(GetHttpV2Client(), commitmentsConfiguration, logger, configuration);
        });

        services.AddTransient<IApprenticeshipLevyApiClient>(provider =>
        {
            var financeConfiguration = provider.GetService<EmployerFinanceConfiguration>();
            return new ApprenticeshipLevyApiClient(new HttpClient { BaseAddress = new Uri(financeConfiguration.Hmrc.BaseUrl) });
        });

        return services;
    }

    private static HttpClient GetHttpV2Client()
    {
        return new HttpClientBuilder()
            .WithHandler(new RequestIdMessageRequestHandler())
            .WithHandler(new SessionIdMessageRequestHandler())
            .WithDefaultHeaders()
            .Build();
    }
}