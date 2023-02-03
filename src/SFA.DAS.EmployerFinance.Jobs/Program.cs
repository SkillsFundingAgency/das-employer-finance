using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.AutoConfiguration;
using SFA.DAS.EmployerFinance.Jobs.DependencyResolution;
using SFA.DAS.EmployerFinance.Startup;
using Microsoft.ApplicationInsights.Extensibility;
using System.Configuration;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.EmployerFinance.Jobs
{
    public class Program
    {
        public static async Task Main()
        {
            using (var container = IoC.Initialize())
            {
                var config = new JobHostConfiguration { JobActivator = new StructureMapJobActivator(container) };
                var loggerFactory = container.GetInstance<ILoggerFactory>();
                var startup = container.GetInstance<IStartup>();

                if (container.GetInstance<IEnvironmentService>().IsCurrent(DasEnv.LOCAL))
                {
                    config.UseDevelopmentSettings();
                }

                config.LoggerFactory = loggerFactory;
                config.UseTimers();

                var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;

                await startup.StartAsync();

                jobHost.RunAndBlock();

                await startup.StopAsync();
            }
        }
    }
}