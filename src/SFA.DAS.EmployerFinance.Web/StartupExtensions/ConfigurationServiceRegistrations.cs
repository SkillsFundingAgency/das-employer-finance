using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmployerFinanceWebConfiguration>(configuration.GetSection(nameof(EmployerFinanceConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFinanceWebConfiguration>>().Value);

        services.AddSingleton<EmployerFinanceConfiguration>(sp => sp.GetService<EmployerFinanceWebConfiguration>());

        services.Configure<ZenDeskConfiguration>(configuration.GetSection(nameof(ZenDeskConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<ZenDeskConfiguration>>().Value);
            
        services.Configure<IAccountApiConfiguration>(configuration.GetSection(nameof(AccountApiConfiguration)));
        services.AddSingleton<IAccountApiConfiguration>(configuration.GetSection(nameof(AccountApiConfiguration)).Get<AccountApiConfiguration>());

        services.Configure<CommitmentsApiV2ClientConfiguration>(configuration.GetSection(nameof(CommitmentsApiV2ClientConfiguration)));
        services.AddSingleton(configuration.GetSection(nameof(CommitmentsApiV2ClientConfiguration)).Get<CommitmentsApiV2ClientConfiguration>());
        services.Configure<EmployerFinanceOuterApiConfiguration>(configuration.GetSection(nameof(EmployerFinanceOuterApiConfiguration)));
        services.AddSingleton(sp => sp.GetService<IOptions<EmployerFinanceOuterApiConfiguration>>().Value);

        var encodingConfigJson = configuration.GetSection(ConfigurationKeys.EncodingConfig).Value;
        var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
        services.AddSingleton(encodingConfig);

        return services;
    }
}