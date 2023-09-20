using Microsoft.Azure.WebJobs.Logging.ApplicationInsights;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerFinance.Commands.RenameAccount;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.MessageHandlers.ServiceRegistrations;
using SFA.DAS.EmployerFinance.MessageHandlers.Startup;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.Notifications.Api.Client.Configuration;
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
            var connectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                loggingBuilder.AddNLog(context.HostingEnvironment.IsDevelopment() ? "nlog.development.config" : "nlog.config");
                loggingBuilder.AddApplicationInsightsWebJobs(o => o.ConnectionString = connectionString);
                
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
            }

            loggingBuilder.AddConsole();
        });

        return builder;
    }

    public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder hostBuilder)
    {

        return hostBuilder.ConfigureAppConfiguration((context, builder) =>
        {
            builder
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            builder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = new[]
                    {
                        ConfigurationKeys.EmployerFinanceJobs,
                        ConfigurationKeys.EncodingConfig,
                        $"{ConfigurationKeys.FinanceNotifications}:{nameof(NotificationsApiClientConfiguration)}"
                    };
                    options.PreFixConfigurationKeys = false;
                    options.ConfigurationKeysRawJsonResult = new[] { ConfigurationKeys.EncodingConfig };
                }
            );
            builder.Build();
        });
    }

    public static IHostBuilder ConfigureDasServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddNotifications(context.Configuration);
            services.AddConfigurationSections(context.Configuration);
            services.AddClientRegistrations();
            services.AddNServiceBus();
            services.AddDataRepositories();
            services.AddApplicationServices();
            services.AddDatabaseRegistration();
            services.AddMediatR(typeof(RenameAccountCommand));
            services.AddAutoMapper(typeof(TransactionRepository));
            services.AddUnitOfWork();
            services.AddMediatorValidators();
            services.AddHmrcServices();
            services.AddProviderServices();
            services.AddEventsApi();
            services.AddCachesRegistrations(context.Configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase));
            services.AddEmployerFinanceOuterApi();
            services.AddTransient<IRetryStrategy>(_ => new ExponentialBackoffRetryAttribute(5, "00:00:10", "00:00:20"));
        });

        return hostBuilder;
    }
}