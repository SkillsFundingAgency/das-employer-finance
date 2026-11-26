using System;
using System.Collections.Generic;
using FluentAssertions;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using NServiceBus;
using NServiceBus.Transport;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.MessageHandlers.Extensions;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.Extensions;

public class HmrcRecoverabilityPolicyTests
{
    private const string ErrorQueue = "SFA.DAS.EmployerFinance.MessageHandlers-errors";

    private static RecoverabilityConfig CreateConfig()
    {
        return new RecoverabilityConfig(
            new ImmediateConfig(5),
            new DelayedConfig(5, TimeSpan.FromSeconds(5)),
            new FailedConfig(ErrorQueue, new HashSet<Type>()));
    }

    private static ErrorContext CreateErrorContext(Exception exception, int delayedDeliveriesPerformed = 0)
    {
        var headers = new Dictionary<string, string>();
        if (delayedDeliveriesPerformed > 0)
        {
            headers[Headers.DelayedRetries] = delayedDeliveriesPerformed.ToString();
        }

        return new ErrorContext(exception, headers, "msg-1", Array.Empty<byte>(), new TransportTransaction(), 1);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Returns_Thirty_Second_Delayed_Retry_For_429_Before_Limit(int delayedDeliveriesPerformed)
    {
        var exception = new ApiHttpException(429, "TOO MANY REQUESTS", string.Empty, string.Empty);

        var action = HmrcRecoverabilityPolicy.Evaluate(CreateConfig(), CreateErrorContext(exception, delayedDeliveriesPerformed));

        action.Should().BeOfType<DelayedRetry>()
            .Which.Delay.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Test]
    public void Moves_429_To_Error_Queue_After_Three_Delayed_Attempts()
    {
        var exception = new ApiHttpException(429, "TOO MANY REQUESTS", string.Empty, string.Empty);

        var action = HmrcRecoverabilityPolicy.Evaluate(CreateConfig(), CreateErrorContext(exception, 3));

        action.Should().BeOfType<MoveToError>()
            .Which.ErrorQueue.Should().Be(ErrorQueue);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Returns_Two_Minute_Delayed_Retry_For_403_Before_Limit(int delayedDeliveriesPerformed)
    {
        var exception = new ApiHttpException(403, "FORBIDDEN", string.Empty, string.Empty);

        var action = HmrcRecoverabilityPolicy.Evaluate(CreateConfig(), CreateErrorContext(exception, delayedDeliveriesPerformed));

        action.Should().BeOfType<DelayedRetry>()
            .Which.Delay.Should().Be(TimeSpan.FromMinutes(2));
    }

    [Test]
    public void Moves_403_To_Error_Queue_After_Three_Delayed_Attempts()
    {
        var exception = new ApiHttpException(403, "FORBIDDEN", string.Empty, string.Empty);

        var action = HmrcRecoverabilityPolicy.Evaluate(CreateConfig(), CreateErrorContext(exception, 3));

        action.Should().BeOfType<MoveToError>()
            .Which.ErrorQueue.Should().Be(ErrorQueue);
    }

    [Test]
    public void Detects_Wrapped_Hmrc_429_In_Inner_Exception()
    {
        var inner = new ApiHttpException(429, "TOO MANY REQUESTS", string.Empty, string.Empty);
        var wrapped = new InvalidOperationException("Wrapped", inner);

        var action = HmrcRecoverabilityPolicy.Evaluate(CreateConfig(), CreateErrorContext(wrapped));

        action.Should().BeOfType<DelayedRetry>()
            .Which.Delay.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Test]
    public void Detects_Wrapped_Hmrc_403_In_Aggregate_Exception()
    {
        var inner = new ApiHttpException(403, "FORBIDDEN", string.Empty, string.Empty);
        var aggregate = new AggregateException(inner);

        var action = HmrcRecoverabilityPolicy.Evaluate(CreateConfig(), CreateErrorContext(aggregate));

        action.Should().BeOfType<DelayedRetry>()
            .Which.Delay.Should().Be(TimeSpan.FromMinutes(2));
    }

    [TestCase(500)]
    [TestCase(503)]
    public void Falls_Back_To_Default_Immediate_Retry_For_Other_Hmrc_Status_Codes(int httpCode)
    {
        var exception = new ApiHttpException(httpCode, "SERVER ERROR", string.Empty, string.Empty);

        var action = HmrcRecoverabilityPolicy.Evaluate(CreateConfig(), CreateErrorContext(exception));

        action.Should().BeOfType<ImmediateRetry>();
    }

    [Test]
    public void Falls_Back_To_Default_Immediate_Retry_For_Non_Hmrc_Exceptions()
    {
        var exception = new InvalidOperationException("Transient failure");

        var action = HmrcRecoverabilityPolicy.Evaluate(CreateConfig(), CreateErrorContext(exception));

        action.Should().BeOfType<ImmediateRetry>();
    }
}
