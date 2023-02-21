using System.Net.Http;
namespace SFA.DAS.EmployerFinance.Http;

public class HttpResponseLogger : IHttpResponseLogger
{
    private readonly ILogger<HttpResponseLogger> _logger;

    public HttpResponseLogger(ILogger<HttpResponseLogger> logger)
    {
        _logger = logger;
    }

    public async Task LogResponseAsync(HttpResponseMessage response)
    {
        if (IsContentStringType(response))
        {
            var content = await response.Content.ReadAsStringAsync();

            //_logger.LogDebug("Logged response. StatusCode: '{StatusCode}'. Reason: '{Reason}'. Content: '{Content}'", response.StatusCode, response.ReasonPhrase, content);
            _logger.LogDebug("Logged response. StatusCode: '{StatusCode}'. Reason: '{Reason}'. Content: '{Content}'.", response.StatusCode, response.ReasonPhrase, content);
        }
    }

    private static bool IsContentStringType(HttpResponseMessage response)
    {
        return response?.Content?.Headers?.ContentType != null && (
            response.Content.Headers.ContentType.MediaType.StartsWith("text") ||
            response.Content.Headers.ContentType.MediaType == "application/json");
    }
}