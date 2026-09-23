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
    public void IsCommand_MatchesICommandWhenTheNameDoesNotEndWithCommand()
    {
        EndpointConfigurationExtensions.IsCommand(typeof(MarkerCommand)).Should().BeTrue();
    }

    [Test]
    public void IsEvent_MatchesVersionedEventName()
    {
        EndpointConfigurationExtensions.IsEvent(typeof(SampleEventV2)).Should().BeTrue();
    }

    [Test]
    public void IsEvent_MatchesIEventWhenTheNameDoesNotEndWithEvent()
    {
        EndpointConfigurationExtensions.IsEvent(typeof(MarkerEvent)).Should().BeTrue();
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

    private class MarkerEvent : IEvent
    {
    }

    private class MarkerCommand : ICommand
    {
    }

    private class UnrelatedType
    {
    }
}
