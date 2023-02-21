using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Http;

namespace SFA.DAS.EmployerFinance.UnitTests.Http.HttpResponseLoggerTests;

public class WhenIReceiveJsonResponse
{
    private Mock<ILogger<HttpResponseLogger>> _logger;
    private HttpResponseLogger _httpResponseLogger;
    private HttpResponseMessage _httpResponseMessage;

    public class TestClass
    {
        public string Field1 { get; set; }
        public int Field2 { get; set; }
    }

    private readonly TestClass _testContent = new() { Field1 = "Test", Field2 = 123 };

    [SetUp]
    public void Arrange()
    {
        _logger = new Mock<ILogger<HttpResponseLogger>>();
        _httpResponseLogger = new HttpResponseLogger(_logger.Object);
        _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(_testContent)),
            Headers = { { "ContentType", "application/json" } }
        };
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
    public async Task ThenTheJsonContentShouldBeLogged()
    {
        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);

        // Assert
        _logger.VerifyLogging($"Logged response. StatusCode: '{_httpResponseMessage.StatusCode}'. Reason: '{_httpResponseMessage.ReasonPhrase}'. Content: '{JsonConvert.SerializeObject(_testContent)}'.");
    }
}