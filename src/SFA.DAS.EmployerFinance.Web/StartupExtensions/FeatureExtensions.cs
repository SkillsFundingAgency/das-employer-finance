using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class FeatureExtensions
{
    public static void AddFeatureToggle(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFeature>(new Feature(configuration));
    }
}