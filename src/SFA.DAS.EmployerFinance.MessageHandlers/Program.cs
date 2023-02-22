using SFA.DAS.EmployerFinance.MessageHandlers.DependencyResolution;
using SFA.DAS.EmployerFinance.MessageHandlers.Extensions;

namespace SFA.DAS.EmployerFinance.MessageHandlers;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = CreateHost(args);

        await host.RunAsync();
    }

    private static IHost CreateHost(string[] args)
    {
        var builder = new HostBuilder()
            .UseDasEnvironment()
            .ConfigureDasAppConfiguration()
            .ConfigureContainer<Registry>(IoC.Initialize)
            .UseConsoleLifetime()
            .ConfigureDasLogging()
            .ConfigureDasServices()
            .UseStructureMap();


        return builder.Build();
    }
}