using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Jobs.ServiceRegistrations;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;

namespace SFA.DAS.EmployerFinance.Jobs.Extensions;

public static class HostExtensions
{
    public static IHostBuilder ConfigureDasWebJobs(this IHostBuilder builder)
    {
        builder.ConfigureWebJobs(config =>
        {
            config.AddTimers();
            config.AddAzureStorageCoreServices();
        });

        return builder;
    }

    public static IHostBuilder ConfigureDasLogging(this IHostBuilder builder)
    {
        builder.ConfigureLogging((context, loggingBuilder) =>
        {
            var appInsightsKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            if (!string.IsNullOrEmpty(appInsightsKey))
            {
                loggingBuilder.AddNLog(context.HostingEnvironment.IsDevelopment() ? "nlog.development.config" : "nlog.config");
                loggingBuilder.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = appInsightsKey);
            }

            loggingBuilder.AddConsole();
        });

        return builder;
    }

    public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureAppConfiguration((context, builder) =>
        {
            builder.AddAzureTableStorage(ConfigurationKeys.EmployerFinance)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
        });
    }

    public static IHostBuilder ConfigureDasServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddConfigurationSections(context.Configuration);
            services.AddNServiceBus();
            services.AddDataRepositories();
            services.AddTransient<IRetryStrategy>(_ => new ExponentialBackoffRetryAttribute(5, "00:00:10", "00:00:20"));
            services.AddUnitOfWork();
#pragma warning disable 618
            services.AddSingleton<IWebHookProvider>(p => null);
#pragma warning restore 618}
        });

        return hostBuilder;
    }
}