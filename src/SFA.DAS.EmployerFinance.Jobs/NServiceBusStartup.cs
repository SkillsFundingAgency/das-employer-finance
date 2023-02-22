using System.Net;
using NServiceBus;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Startup;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.SqlServer.Configuration;

namespace SFA.DAS.EmployerFinance.Jobs;

public class NServiceBusStartup : IStartup
{
    private readonly IContainer _container;
    private IEndpointInstance _endpoint;

    public NServiceBusStartup(IContainer container)
    {
        _container = container;
    }

    public async Task StartAsync()
    {
        var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFinance.Jobs")
                .UseErrorQueue("SFA.DAS.EmployerFinance.Jobs-errors")
                .UseAzureServiceBusTransport(() => _container.GetInstance<EmployerFinanceConfiguration>().ServiceBusConnectionString, _container)
                .UseLicense(WebUtility.HtmlDecode(_container.GetInstance<EmployerFinanceConfiguration>().NServiceBusLicense))
                .UseSqlServerPersistence(() => _container.GetInstance<DbConnection>())
                .UseNewtonsoftJsonSerializer()
                //MAP-192 Need implementing
                //.UseNLogFactory()
                .UseSendOnly()
            //.UseStructureMapBuilder(_container)
            ;

        _endpoint = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

        _container.Configure(c =>
        {
            c.For<IMessageSession>().Use(_endpoint);
        });
    }

    public async Task StopAsync()
    {
        await _endpoint.Stop();
    }
}