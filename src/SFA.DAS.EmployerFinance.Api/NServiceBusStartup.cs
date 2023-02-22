using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
//using SFA.DAS.NServiceBus.Configuration.StructureMap;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using StructureMap;
//using WebApi.StructureMap;

namespace SFA.DAS.EmployerFinance.Api
{
    public class NServiceBusStartup //: IStartup
    {
        private readonly IContainer _container;
        private readonly EmployerFinanceConfiguration _employerFinanceConfiguration;
        private readonly IConfiguration _configuration;
        private IEndpointInstance _endpoint;

        public NServiceBusStartup(IContainer container, EmployerFinanceConfiguration employerFinanceConfiguration, IConfiguration configuration)
        {
            _container = container;
            _employerFinanceConfiguration = employerFinanceConfiguration;
            _configuration = configuration;
        }

        public async Task StartAsync()
        {

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFinance.Api")
                .UseAzureServiceBusTransport(() => _employerFinanceConfiguration.ServiceBusConnectionString, _container, _configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
                .UseErrorQueue("SFA.DAS.EmployerFinance.Api-errors")
                .UseInstallers()
                .UseLicense(WebUtility.HtmlDecode(_employerFinanceConfiguration.NServiceBusLicense))
                .UseSqlServerPersistence(() => _container.GetInstance<DbConnection>())
                .UseNewtonsoftJsonSerializer()                
                //.UseNLogFactory()
                .UseOutbox()
                //.UseStructureMapBuilder(container)
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