using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.AutoConfiguration;
using SFA.DAS.EmployerFinance.Jobs.DependencyResolution;
using SFA.DAS.EmployerFinance.Startup;
using Microsoft.ApplicationInsights.Extensibility;
using System.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace SFA.DAS.EmployerFinance.Jobs
{
    public class Program
    {
        public static async Task Main()
        {

            var container = IoC.Initialize();

            using (var host = CreateHost(container))
            {
                var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;

                await host.StartAsync();

                await host.RunAsync();

                await host.StopAsync();
            }

        }

        private static IHost CreateHost(IContainer container)
        {
            var builder = new HostBuilder()
                 .ConfigureWebJobs(config =>
                 {
                     config.AddTimers();
                 })
                 .ConfigureLogging((context, loggingBuilder) =>
                 {
                     var appInsightsKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                     if (!string.IsNullOrEmpty(appInsightsKey))
                     {
                         loggingBuilder.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = appInsightsKey);
                     }
                 }).ConfigureServices((context, services) =>
                 {
                     services.AddScoped<IJobActivator, StructureMapJobActivator>();
                 });

            var isDevelopment = container.GetInstance<IEnvironmentService>().IsCurrent(DasEnv.LOCAL);

            if (isDevelopment)
            {
                builder.UseEnvironment("development");
            }

            return builder.Build();
        }
    }
}