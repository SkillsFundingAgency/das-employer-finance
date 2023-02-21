using System.Net;
using System.Net.Http;
using SFA.DAS.EmployerFinance.Http;

namespace SFA.DAS.EmployerFinance.UnitTests.Http.HttpResponseLoggerTests;

public class WhenIReceiveAStringResponse
{
    private Mock<ILogger<HttpResponseLogger>> _logger;
    private HttpResponseLogger _httpResponseLogger;
    private HttpResponseMessage _httpResponseMessage;

    private const string TestContent = "Some test content";
    private const HttpStatusCode TestStatusCode = HttpStatusCode.BadRequest;
    private const string TestReason = "Some error summary";

    [SetUp]
    public void Arrange()
    {
        _logger = new Mock<ILogger<HttpResponseLogger>>();
        _httpResponseLogger = new HttpResponseLogger(_logger.Object);
        _httpResponseMessage = new HttpResponseMessage(TestStatusCode) { Content = new StringContent(TestContent), ReasonPhrase = TestReason };
    }

    [Test]
    public async Task ThenTheContentShouldBeLogged()
    {
        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);

        // Assert
        _logger.VerifyLogging(It.IsAny<string>(), LogLevel.Debug, Times.Never());
    }

    [Test]
    public async Task ThenTheMessageShouldBeLogged()
    {
        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);

        // Assert
        _logger.VerifyLogging($"Logged response. StatusCode: '{TestStatusCode}'. Reason: '{TestReason}'. Content: '{TestContent}'.");
    }
}