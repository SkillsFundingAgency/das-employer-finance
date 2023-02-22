﻿using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Jobs.DependencyResolution;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;

namespace SFA.DAS.EmployerFinance.Jobs.Extensions;

public static class HostExtensions
{
    private const string AzureResource = "https://database.windows.net/";

    public static IHostBuilder UseStructureMap(this IHostBuilder builder)
    {
        return UseStructureMap(builder, registry: null);
    }

    public static IHostBuilder UseStructureMap(this IHostBuilder builder, Registry registry)
    {
        return builder.UseServiceProviderFactory(new StructureMapServiceProviderFactory(registry));
    }

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
        hostBuilder.ConfigureServices(services =>
        {
            services.AddNServiceBus();
            services.AddScoped(GetDbContext);
            services.AddDataRepositories();
            services.AddScoped<IJobActivator, StructureMapJobActivator>();
            services.AddTransient<IRetryStrategy>(_ => new ExponentialBackoffRetryAttribute(5, "00:00:10", "00:00:20"));
            services.AddUnitOfWork();
#pragma warning disable 618
            services.AddSingleton<IWebHookProvider>(p => null);
#pragma warning restore 618}
        });

        return hostBuilder;
    }

    private static EmployerFinanceDbContext GetDbContext(IServiceProvider serviceProvider)
    {
        var employerFinanceConfiguration = serviceProvider.GetService<EmployerFinanceConfiguration>();
        var configuration = serviceProvider.GetService<IConfiguration>();

        var environmentName = configuration["EnvironmentName"];

        var azureServiceTokenProvider = new AzureServiceTokenProvider();

        var connectionString = employerFinanceConfiguration.DatabaseConnectionString;

        var connection = environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase)
            ? new SqlConnection(connectionString)
            : new SqlConnection
            {
                ConnectionString = connectionString,
                AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result
            };

        var optionsBuilder = new DbContextOptionsBuilder<EmployerFinanceDbContext>();
        optionsBuilder.UseSqlServer(connection);

        return new EmployerFinanceDbContext(optionsBuilder.Options);
    }

    private static string GetConnectionString(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<EmployerFinanceConfiguration>().DatabaseConnectionString;
    }
}