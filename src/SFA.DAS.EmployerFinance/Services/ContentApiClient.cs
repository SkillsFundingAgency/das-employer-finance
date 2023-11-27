using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class ContentApiClient : IContentApiClient
{
    private readonly string _apiBaseUrl;
    private readonly string _identifierUri;
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;

    public ContentApiClient(HttpClient client, IContentClientApiConfiguration contentClientConfiguration,
        IConfiguration configuration)
    {
        _apiBaseUrl = contentClientConfiguration.ApiBaseUrl.EndsWith("/")
            ? contentClientConfiguration.ApiBaseUrl
            : contentClientConfiguration.ApiBaseUrl + "/";

        _identifierUri = contentClientConfiguration.IdentifierUri;
        _client = client;
        _configuration = configuration;
    }
    
    public async Task<string> Get(string type, string applicationId)
    {
        var uri = $"{_apiBaseUrl}api/content?applicationId={applicationId}&type={type}";

        return await GetAsync(uri);
    }
    
    private async Task<string> GetAsync(string url)
    {
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(httpRequestMessage);
        
        using var response = await _client.SendAsync(httpRequestMessage);
        var result = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        return result;
    }
    
    private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
    {
        if (_configuration["EnvironmentName"] != "LOCAL")
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(_identifierUri);

            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}