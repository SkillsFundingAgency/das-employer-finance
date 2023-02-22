using NServiceBus;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.StructureMap;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;

namespace SFA.DAS.EmployerFinance.Jobs.Extensions;

public static class ServiceCollectionExtensions
{
    private const string EndpointName = "SFA.DAS.EmployerFinance.Jobs";

    public static IServiceCollection AddNServiceBus(this IServiceCollection services)
    {
        return services
            .AddSingleton(p =>
            {
                var container = p.GetService<IContainer>();
                var hostingEnvironment = p.GetService<IHostEnvironment>();
                var configuration = p.GetService<EmployerFinanceConfiguration>();
                var isDevelopment = hostingEnvironment.IsDevelopment();

                var endpointConfiguration = new EndpointConfiguration(EndpointName)
                    .UseErrorQueue($"{EndpointName}-errors")
                    .UseInstallers()
                    .UseSendOnly()
                    .UseLicense(configuration.NServiceBusLicense)
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                    .UseSqlServerPersistence(() => container.GetInstance<DbConnection>())
                    .UseSendOnly()
                    .UseStructureMapBuilder(container);

                if (isDevelopment)
                {
                    endpointConfiguration.UseLearningTransport();
                }
                else
                {
                    endpointConfiguration.UseAzureServiceBusTransport(configuration.ServiceBusConnectionString);
                }
                    
                var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                return endpoint;
            })
            .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
            .AddHostedService<NServiceBusHostedService>();
    }
}