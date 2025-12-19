using NServiceBus;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
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
                var employerFinanceConfiguration = p.GetService<EmployerFinanceConfiguration>();
                var configuration = p.GetService<IConfiguration>();
                var isLocal = configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase);
                
                var logger = p.GetService<ILogger<Program>>();
                logger.LogInformation(employerFinanceConfiguration.SqlConnectionString);
                logger.LogInformation(employerFinanceConfiguration.ServiceBusConnectionString);
                
                var endpointConfiguration = new EndpointConfiguration(EndpointName)
                    .UseErrorQueue($"{EndpointName}-errors")
                    .UseInstallers()
                    .UseLicense(employerFinanceConfiguration.NServiceBusLicense)
                    .UseNewMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                    .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(employerFinanceConfiguration.SqlConnectionString))
                    .UseAzureServiceBusTransport(() => employerFinanceConfiguration.ServiceBusConnectionString, isLocal)
                    .UseSendOnly();

                var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                return endpoint;
            })
            .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
            .AddHostedService<NServiceBusHostedService>();
    }
}