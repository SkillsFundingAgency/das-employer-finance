using SFA.DAS.EmployerFinance.MessageHandlers.Extensions;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;

namespace SFA.DAS.EmployerFinance.MessageHandlers;

public class Program
{
    public static async Task Main(string[] args)
    {
        using (var host = CreateHost(args))
        {
            await host.RunAsync();
        }
    }

    private static IHost CreateHost(string[] args)
    {
        var builder = new HostBuilder()
            .ConfigureDasAppConfiguration()
            .UseDasEnvironment()
            .UseConsoleLifetime()
            .ConfigureDasLogging()
            .ConfigureDasServices()
            .UseNServiceBusContainer();

        return builder.Build();
    }
}