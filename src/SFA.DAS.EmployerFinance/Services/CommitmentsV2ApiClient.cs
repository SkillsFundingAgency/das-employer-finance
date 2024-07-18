using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class CommitmentsV2ApiClient(
    HttpClient httpClient,
    CommitmentsApiV2ClientConfiguration commitmentsApiClientConfig,
    ILogger<CommitmentsV2ApiClient> logger,
    IConfiguration configuration)
    : ICommitmentsV2ApiClient
{
    private readonly AzureServiceTokenProvider _azureServiceTokenProvider = new();

    public async Task<GetApprenticeshipResponse> GetApprenticeship(long apprenticeshipId)
    {
        var url = $"{BaseUrl()}api/apprenticeships/{apprenticeshipId}";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonConvert.DeserializeObject<GetApprenticeshipResponse>(json);
    }

    public async Task<GetTransferRequestSummaryResponse> GetTransferRequests(long accountId)
    {
        var url = $"{BaseUrl()}api/accounts/{accountId}/transfers";
        logger.LogInformation("Getting GetTransferRequests {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonConvert.DeserializeObject<GetTransferRequestSummaryResponse>(json);
    }

    private string BaseUrl()
    {
        if (commitmentsApiClientConfig.ApiBaseUrl.EndsWith("/"))
        {
            return commitmentsApiClientConfig.ApiBaseUrl;
        }

        return commitmentsApiClientConfig.ApiBaseUrl + "/";
    }

    private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
    {
        if (configuration["EnvironmentName"].ToUpper() != "LOCAL")
        {
            var accessToken = await _azureServiceTokenProvider.GetAccessTokenAsync(commitmentsApiClientConfig.IdentifierUri);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}