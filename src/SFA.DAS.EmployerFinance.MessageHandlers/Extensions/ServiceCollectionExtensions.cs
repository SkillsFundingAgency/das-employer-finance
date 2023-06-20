using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;

namespace SFA.DAS.EmployerFinance.MessageHandlers.Extensions;

public static class ServiceCollectionExtensions
{
    private const string EndpointName = "SFA.DAS.EmployerFinance.MessageHandlers";

    public static IServiceCollection AddNServiceBus(this IServiceCollection services)
    {
        return services
            .AddSingleton(p =>
            {
                var employerFinanceConfiguration = p.GetService<EmployerFinanceConfiguration>();
                var configuration = p.GetService<IConfiguration>();
                var isLocal = configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase);

                var logger = p.GetService<ILogger<Program>>();
                logger.LogInformation(employerFinanceConfiguration.DatabaseConnectionString);
                logger.LogInformation(employerFinanceConfiguration.ServiceBusConnectionString);

                var endpointConfiguration = new EndpointConfiguration(EndpointName)
                    .UseErrorQueue($"{EndpointName}-errors")
                    .UseInstallers()
                    .UseOutbox()    
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                    .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(employerFinanceConfiguration.DatabaseConnectionString))
                    .UseAzureServiceBusTransport(() => employerFinanceConfiguration.ServiceBusConnectionString, isLocal)
                    .UseServicesBuilder(new UpdateableServiceProvider(services))
                    .UseUnitOfWork();
                
                if (!string.IsNullOrEmpty(employerFinanceConfiguration.NServiceBusLicense))
                {
                    endpointConfiguration.UseLicense(employerFinanceConfiguration.NServiceBusLicense);
                }
                
                var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                return endpoint;
            })
            .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
            .AddHostedService<NServiceBusHostedService>();
    }
}