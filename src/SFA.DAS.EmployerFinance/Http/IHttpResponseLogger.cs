using System.Net.Http;

namespace SFA.DAS.EmployerFinance.Http;

public interface IHttpResponseLogger
{
    Task LogResponseAsync(HttpResponseMessage response);
}