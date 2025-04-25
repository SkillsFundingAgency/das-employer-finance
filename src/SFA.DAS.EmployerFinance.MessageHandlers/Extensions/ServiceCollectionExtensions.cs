using Azure.Messaging.ServiceBus;
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
                    .UseNewtonsoftJsonSerializer()
                    .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(employerFinanceConfiguration.DatabaseConnectionString))
                    .UseAzureServiceBusTransport(() => employerFinanceConfiguration.ServiceBusConnectionString, isLocal)
                    .UseServicesBuilder(new UpdateableServiceProvider(services))
                    .UseMetrics();

                endpointConfiguration.UseUnitOfWork();
                
                var recoverability = endpointConfiguration.Recoverability();
                recoverability.Immediate(immediate => immediate.NumberOfRetries(5));
                recoverability.Delayed(delayed => 
                {
                    delayed.NumberOfRetries(5);
                    delayed.TimeIncrease(TimeSpan.FromSeconds(5));
                });

                endpointConfiguration.LimitMessageProcessingConcurrencyTo(1);
                
                if (!string.IsNullOrEmpty(employerFinanceConfiguration.NServiceBusLicense))
                {
                    endpointConfiguration.UseLicense(employerFinanceConfiguration.NServiceBusLicense);
                }
                
                var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                return endpoint;
            })
            .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
            .AddScoped(x =>
            {
                var config = x.GetService<EmployerFinanceJobsConfiguration>();
                return new ServiceBusClient(config.ServiceBusConnectionString);
            })
            .AddHostedService<NServiceBusHostedService>();
    }
}