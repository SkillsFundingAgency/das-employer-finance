namespace SFA.DAS.EmployerFinance.Interfaces.OuterApi;

public interface IOuterApiClient
{
    Task<TResponse> Get<TResponse>(IGetApiRequest request, CancellationToken cancellationToken = default);
    Task<TResponse> Post<TResponse>(string url, object body, CancellationToken cancellationToken = default);
}