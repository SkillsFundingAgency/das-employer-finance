using SFA.DAS.EmployerFinance.Jobs.DependencyResolution;
using SFA.DAS.EmployerFinance.Jobs.Extensions;

namespace SFA.DAS.EmployerFinance.Jobs;

public class Program
{
    public static async Task Main()
    {
        using (var host = CreateHost())
        {
            await host.RunAsync();
        }
    }

    //private static IHost CreateHostOld(IContainer container)
    //{
    //    var builder = new HostBuilder()
    //        .ConfigureWebJobs(config =>
    //        {
    //            config.AddTimers();
    //        })
    //        .ConfigureLogging((context, loggingBuilder) =>
    //        {
    //            var appInsightsKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
    //            if (!string.IsNullOrEmpty(appInsightsKey))
    //            {
    //                loggingBuilder.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = appInsightsKey);
    //            }
    //        }).ConfigureServices((context, services) =>
    //        {
    //            services.AddScoped<IJobActivator, StructureMapJobActivator>();
    //        });

    //    var isDevelopment = container.GetInstance<IEnvironmentService>().IsCurrent(DasEnv.LOCAL);

    //    if (isDevelopment)
    //    {
    //        builder.UseEnvironment("development");
    //    }

    //    return builder.Build();
    //}

    private static IHost CreateHost()
    {
        return new HostBuilder()
            .ConfigureDasWebJobs()
            .ConfigureDasLogging()
            .ConfigureDasServices()
            .UseStructureMap()
            .ConfigureContainer<Registry>(IoC.Initialize)
            .Build();
    }
}