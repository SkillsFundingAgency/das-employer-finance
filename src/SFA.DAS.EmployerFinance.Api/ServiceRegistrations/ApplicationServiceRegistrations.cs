using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Helpers;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Time;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.ServiceRegistrations;

public static class ApplicationServiceRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IEncodingService, EncodingService>();
        services.AddTransient<IDasLevyService, DasLevyService>();
        services.AddTransient<ICurrentDateTime, CurrentDateTime>();

        services.AddSingleton<IHmrcDateService, HmrcDateService>();
        services.AddScoped<ILinkGeneratorWrapper, LinkGeneratorWrapper>();
        services.AddTransient<IHmrcService, HmrcService>();

        return services;
    }
}