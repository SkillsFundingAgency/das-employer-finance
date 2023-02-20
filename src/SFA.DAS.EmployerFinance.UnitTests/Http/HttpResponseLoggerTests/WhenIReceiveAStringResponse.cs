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
        _logger = new Moq.Mock<ILogger<HttpResponseLogger>>();
        _httpResponseLogger = new HttpResponseLogger(Mock.Of<ILogger<HttpResponseLogger>>());
        _httpResponseMessage = new HttpResponseMessage(TestStatusCode) { Content = new StringContent(TestContent), ReasonPhrase = TestReason};
    }

    [Test]
    public async Task ThenTheContentShouldBeLogged()
    {
        // Arrange
        _logger.Setup(l => l.LogDebug(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()));

        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);

        // Assert
        _logger.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<Dictionary<string,object>>()), Times.Once);
    }

    [TestCase("Content", TestContent)]
    [TestCase("StatusCode", TestStatusCode)]
    [TestCase("Reason", TestReason)]
    public async Task ThenTheMessageShouldBeLogged(string expectedPropertyName, object expectedPropertyValue)
    {
        // Arrange
        IDictionary<string, object> actualProperties = null;
        _logger
            .Setup(l => l.LogDebug(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Callback<string, IDictionary<string, object>>((msg, properties) => actualProperties = properties);

        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);

        // Assert
        Assert.IsTrue(actualProperties.ContainsKey(expectedPropertyName), $"logger was not supplied property {expectedPropertyName}");
        var actualPropertyValue = actualProperties[expectedPropertyName];
        Assert.IsNotNull(actualPropertyValue, $"logger was supplied null for property {expectedPropertyName}");
        Assert.AreEqual(expectedPropertyValue.ToString(), actualPropertyValue.ToString(), $"logger was supplied an unexpected value for property {expectedPropertyName}");
    }
}