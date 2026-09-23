using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.NServiceBus.ClientOutbox;
using EndpointConfigurationExtensions = SFA.DAS.EmployerFinance.Extensions.EndpointConfigurationExtensions;

namespace SFA.DAS.EmployerFinance.UnitTests.Extensions;

[TestFixture]
public class EndpointConfigurationExtensionsTests
{
    [Test]
    public void IsCommand_MatchesProcessClientOutboxMessageCommandV2()
    {
        EndpointConfigurationExtensions.IsCommand(typeof(ProcessClientOutboxMessageCommandV2)).Should().BeTrue();
    }

    [Test]
    public void IsCommand_MatchesProcessClientOutboxMessageCommand()
    {
        EndpointConfigurationExtensions.IsCommand(typeof(ProcessClientOutboxMessageCommand)).Should().BeTrue();
    }

    [Test]
    public void IsCommand_MatchesMessagesCommandsType()
    {
        EndpointConfigurationExtensions.IsCommand(typeof(ExpireFundsCommand)).Should().BeTrue();
    }

    [Test]
    public void IsCommand_DoesNotMatchICommandWhenTheNameDoesNotEndWithCommand()
    {
        EndpointConfigurationExtensions.IsCommand(typeof(OutboxMarker)).Should().BeFalse();
    }

    [Test]
    public void IsEvent_DoesNotMatchAVersionedEventName()
    {
        EndpointConfigurationExtensions.IsEvent(typeof(SampleEventV2)).Should().BeFalse();
    }

    [Test]
    public void IsEvent_DoesNotMatchIEventWhenTheNameDoesNotEndWithEvent()
    {
        EndpointConfigurationExtensions.IsEvent(typeof(PublishedMarker)).Should().BeFalse();
    }

    [Test]
    public void IsEvent_MatchesMessagesEventsType()
    {
        EndpointConfigurationExtensions.IsEvent(typeof(SentTransferConnectionRequestEvent)).Should().BeTrue();
    }

    [Test]
    public void IsCommand_DoesNotMatchAnUnrelatedType()
    {
        EndpointConfigurationExtensions.IsCommand(typeof(UnrelatedType)).Should().BeFalse();
    }

    private class SampleEventV2
    {
    }

    private class PublishedMarker : IEvent
    {
    }

    private class OutboxMarker : ICommand
    {
    }

    private class UnrelatedType
    {
    }
}
