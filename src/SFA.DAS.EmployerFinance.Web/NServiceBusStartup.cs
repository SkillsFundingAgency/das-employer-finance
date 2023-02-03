using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using StructureMap;

namespace SFA.DAS.EmployerFinance.Web
{
    public class NServiceBusStartup // : IStartup
    {
        private readonly StructureMap.IContainer _container;
        private readonly EmployerFinanceConfiguration _employerFinanceConfiguration;
        private IEndpointInstance _endpoint;

        public NServiceBusStartup(StructureMap.IContainer container, EmployerFinanceConfiguration employerFinanceConfiguration, IEndpointInstance endpoint)
        {
            _container = container;
            _employerFinanceConfiguration = employerFinanceConfiguration;
            _endpoint = endpoint;
        }

        public async Task StartAsync()
        {

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerAccounts.Web")
               .UseAzureServiceBusTransport(() => _employerFinanceConfiguration.ServiceBusConnectionString, _container)
                .UseErrorQueue("SFA.DAS.EmployerAccounts.Web-errors")
                .UseInstallers()
                .UseLicense(WebUtility.HtmlDecode(_employerFinanceConfiguration.NServiceBusLicense))
                .UseSqlServerPersistence(() => _container.GetInstance<DbConnection>())
                .UseNewtonsoftJsonSerializer()
                //Map-192 need implementing
                //.UseNLogFactory()
                .UseOutbox(true)
                //.UseStructureMapBuilder(_container)
                .UseUnitOfWork();

            _endpoint = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            _container.Configure(c =>
            {
                c.For<IMessageSession>().Use(_endpoint);
            });
        }

        public Task StopAsync()
        {
            return _endpoint.Stop();
        }
    }
}