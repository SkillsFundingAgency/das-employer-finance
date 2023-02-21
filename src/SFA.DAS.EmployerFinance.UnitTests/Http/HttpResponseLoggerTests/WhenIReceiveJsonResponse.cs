﻿using System.Net;
using System.Net.Http;
using SFA.DAS.EmployerFinance.Http;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.UnitTests.Http.HttpResponseLoggerTests;

public class WhenIReceiveJsonResponse
{
    private Mock<ILog> _logger;
    private HttpResponseLogger _httpResponseLogger;
    private HttpResponseMessage _httpResponseMessage;

    public class TestClass
    {
        public string Field1 { get; set; }
        public int Field2 { get; set; }
    }

    private readonly TestClass _testContent = new TestClass { Field1 = "Test", Field2 = 123 };

    [SetUp]
    public void Arrange()
    {
        _logger = new Mock<ILog>();
        _httpResponseLogger = new HttpResponseLogger(Mock.Of<ILogger<HttpResponseLogger>>());
        _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(_testContent)),
            Headers =
            {
                {"ContentType", "application/json"}
            }
        };
    }

    [Test]
    public async Task ThenTheContentShouldBeLogged()
    {
        // Arrange
        _logger.Setup(l => l.Debug(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()));

        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);

        // Assert
        _logger.Verify(l => l.Debug(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }

    public async Task ThenTheJsonContentShouldBeLogged()
    {
        // Arrange
        IDictionary<string, object> actualProperties = null;
        _logger
            .Setup(l => l.Debug(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Callback<string, IDictionary<string, object>>((msg, properties) => actualProperties = properties);

        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);
        var content = actualProperties["Content"].ToString();
        var rehydratedClass = Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(content);

        // Assert
        Assert.IsNotNull(rehydratedClass);
        Assert.AreEqual(_testContent.Field1, rehydratedClass.Field1);
        Assert.AreEqual(_testContent.Field2, rehydratedClass.Field2);
    }
}