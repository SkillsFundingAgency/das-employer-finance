using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.MessageHandlers.Http;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.Http;

public class HmrcRateLimitingHandlerTests
{
    [Test]
    public async Task Waits_For_Rate_Limiter_Before_Forwarding_Request()
    {
        var callOrder = new List<string>();
        var rateLimiter = new Mock<IHmrcRateLimiter>();
        rateLimiter
            .Setup(x => x.WaitForAvailabilityAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("limiter"))
            .ReturnsAsync(TimeSpan.Zero);

        var innerHandler = new StubHttpMessageHandler(() =>
        {
            callOrder.Add("inner");
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var client = new HttpClient(new HmrcRateLimitingHandler(rateLimiter.Object, Mock.Of<ILogger<HmrcRateLimitingHandler>>(), innerHandler));
        var response = await client.GetAsync("https://hmrc.test/apprenticeship-levy");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        callOrder.Should().ContainInOrder("limiter", "inner");
    }

    [Test]
    public async Task Does_Not_Call_Inner_Handler_When_Cancelled_During_Rate_Limit_Wait()
    {
        var innerCalled = false;
        var rateLimiter = new Mock<IHmrcRateLimiter>();
        rateLimiter
            .Setup(x => x.WaitForAvailabilityAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var innerHandler = new StubHttpMessageHandler(() =>
        {
            innerCalled = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var client = new HttpClient(new HmrcRateLimitingHandler(rateLimiter.Object, Mock.Of<ILogger<HmrcRateLimitingHandler>>(), innerHandler));
        var act = async () => await client.GetAsync("https://hmrc.test/apprenticeship-levy");

        await act.Should().ThrowAsync<OperationCanceledException>();
        innerCalled.Should().BeFalse();
    }

    [Test]
    public void Throws_When_Rate_Limiter_Is_Null()
    {
        var act = () => new HmrcRateLimitingHandler(null, Mock.Of<ILogger<HmrcRateLimitingHandler>>());

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Throws_When_Logger_Is_Null()
    {
        var act = () => new HmrcRateLimitingHandler(Mock.Of<IHmrcRateLimiter>(), null);

        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder());
        }
    }
}
