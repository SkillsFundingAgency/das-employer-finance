using NLog.Extensions.Logging;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;
using SFA.DAS.EmployerFinance.MessageHandlers.Startup;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;

namespace SFA.DAS.EmployerFinance.MessageHandlers.Extensions;

public static class HostExtensions
{
    public static IHostBuilder UseDasEnvironment(this IHostBuilder hostBuilder)
    {
        var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName);
        var mappedEnvironmentName = DasEnvironmentName.Map[environmentName];

        return hostBuilder.UseEnvironment(mappedEnvironmentName);
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
            services.AddClientRegistrations();
            services.AddNServiceBus();
            services.AddDataRepositories();
            services.AddApplicationServices();
            services.AddDatabaseRegistration(context.Configuration.GetConnectionString("DatabaseConnectionString"));
            services.AddMediatR(typeof(Program));
            services.AddUnitOfWork();
            services.AddTransient<IRetryStrategy>(_ => new ExponentialBackoffRetryAttribute(5, "00:00:10", "00:00:20"));
            services.BuildServiceProvider();
        });

        return hostBuilder;
    }
}