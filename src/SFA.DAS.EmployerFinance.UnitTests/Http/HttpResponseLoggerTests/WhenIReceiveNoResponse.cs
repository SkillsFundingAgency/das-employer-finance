﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Http;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.UnitTests.Http.HttpResponseLoggerTests;

public class WhenIReceiveNoResponse
{
    private Mock<ILog> _logger;
    private HttpResponseLogger _httpResponseLogger;
    private HttpResponseMessage _httpResponseMessage;

    [SetUp]
    public void Arrange()
    {
        _logger = new Mock<ILog>();
        _httpResponseLogger = new HttpResponseLogger(Mock.Of<ILogger<HttpResponseLogger>>());
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
        // Arrange
        _logger.Setup(l => l.Debug(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()));

        // Act
        await _httpResponseLogger.LogResponseAsync(_httpResponseMessage);

        // Assert
        _logger.Verify(l => l.Debug(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }
}