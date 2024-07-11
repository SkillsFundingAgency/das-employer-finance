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
    public async Task<GetApprenticeshipResponse> GetApprenticeship(long apprenticeshipId)
    {
        var url = $"{BaseUrl()}api/apprenticeships/{apprenticeshipId}";
        logger.LogInformation("EmployerFinance Services Getting GetApprenticeship {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);
      
        using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        logger.LogInformation("EmployerFinance Services received response for GetApprenticeship {Url}", url);

        return JsonConvert.DeserializeObject<GetApprenticeshipResponse>(json);
    }

    public async Task<GetTransferRequestSummaryResponse> GetTransferRequests(long accountId)
    {
        var url = $"{BaseUrl()}api/accounts/{accountId}/transfers";
        logger.LogInformation("Getting GetTransferRequests {Url}", url);
        
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
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
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(commitmentsApiClientConfig.IdentifierUri);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}