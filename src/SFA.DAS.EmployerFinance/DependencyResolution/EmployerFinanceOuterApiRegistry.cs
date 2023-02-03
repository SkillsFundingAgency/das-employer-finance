using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using System.Net.Http;
using StructureMap;

namespace SFA.DAS.EmployerFinance.DependencyResolution
{
    public class EmployerFinanceOuterApiRegistry :Registry
    {
        public EmployerFinanceOuterApiRegistry()
        {
            For<EmployerFinanceOuterApiConfiguration>().Use(c => c.GetInstance<EmployerFinanceConfiguration>().EmployerFinanceOuterApiConfiguration).Singleton();
            For<IOuterApiClient>().Use<OuterApiClient>().Ctor<HttpClient>().Is(new HttpClient()).Singleton();
        }
    }
}
