using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SFA.DAS.EmployerFinance.Infrastructure;

public class OuterApiClient : IOuterApiClient
{
    private readonly HttpClient _httpClient;
    private readonly EmployerFinanceOuterApiConfiguration _config;

    public OuterApiClient(
        HttpClient httpClient,
        EmployerFinanceOuterApiConfiguration options)
    {
        _httpClient = httpClient;
        _config = options;
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
    }

    public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
            
        AddHeaders(httpRequestMessage);

        var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<TResponse>(json);
    }

    public async Task<TResponse> Post<TResponse>(string url, object body)
    {
        var jsonBody = JsonConvert.SerializeObject(body);

        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json")
        };

        AddHeaders(httpRequestMessage);

        var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<TResponse>(json);
    }


    private void AddHeaders(HttpRequestMessage httpRequestMessage)
    {
        httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key);
        httpRequestMessage.Headers.Add("X-Version", "1");
    }
}