using SFA.DAS.EmployerFinance.Data.Contracts;
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