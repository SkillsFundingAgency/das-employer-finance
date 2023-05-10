using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
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
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.UnitOfWork.NServiceBus.Services;
using SFA.DAS.UnitOfWork.Pipeline;

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
        services.AddScoped<ILevyImportCleanerStrategy, LevyImportCleanerStrategy>();
        services.AddScoped<IUnitOfWork, UnitOfWork.NServiceBus.Pipeline.UnitOfWork>();
        services.AddScoped<IEventPublisher, EventPublisher>();

        return services;
    }
}