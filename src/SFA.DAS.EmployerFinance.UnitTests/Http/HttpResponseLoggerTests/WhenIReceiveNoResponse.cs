using System.Net;
using System.Net.Http;
using SFA.DAS.EmployerFinance.Http;

namespace SFA.DAS.EmployerFinance.UnitTests.Http.HttpResponseLoggerTests;

public class WhenIReceiveNoResponse
{
    private Mock<ILogger<HttpResponseLogger>> _logger;
    private HttpResponseLogger _httpResponseLogger;
    private HttpResponseMessage _httpResponseMessage;

    [SetUp]
    public void Arrange()
    {
        _logger = new Mock<ILogger<HttpResponseLogger>>();
        _httpResponseLogger = new HttpResponseLogger(_logger.Object);
        _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = null,
            Headers =
            {
                { "ContentType", "application/json" }
            }
        };
    }

    [Test]
    public async Task ThenTheContentShouldNotBeLogged()
    {
        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);

        // Assert
        _logger.VerifyLogging(It.IsAny<string>(), LogLevel.Debug, Times.Never());
    }
}