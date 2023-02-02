using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Startup;
using SFA.DAS.EmployerFinance.Web.App_Start;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.NLog;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.NServiceBus.Configuration.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using NServiceBus.ObjectBuilder.Common;
using System.Drawing.Printing;

namespace SFA.DAS.EmployerFinance.Web
{
    public class NServiceBusStartup // : IStartup
    {
        private readonly IContainer _container;
        private readonly EmployerFinanceConfiguration _employerFinanceConfiguration;
        private IEndpointInstance _endpoint;

        public NServiceBusStartup(IContainer container, EmployerFinanceConfiguration employerFinanceConfiguration, IEndpointInstance endpoint)
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
                .UseNLogFactory()
                .UseOutbox(true)
                .UseStructureMapBuilder(_container)
                .UseUnitOfWork();

            _endpoint = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            container.Configure(c =>
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