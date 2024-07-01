using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class RejectedTransferConnectionRequestEventNotificationHandlerTests
{
    [Test]
    public async Task Handle_WhenRejectedTransferConnectionRequestEventIsHandled_ThenShouldNotifyAccountOwnersRequiringNotification()
    {
        var fixture = new RejectedTransferConnectionRequestEventNotificationHandlerTestsFixture();

        await fixture.Handle();

        fixture.NotificationsService.Verify(r => r.SendEmail(
                "TransferConnectionRequestRejected",
                It.IsAny<string>(),
                It.Is<Dictionary<string, string>>(tokens =>
                    tokens["link_notification_page"] == $"{fixture.Configuration.EmployerFinanceBaseUrl}accounts/{TransferConnectionRequestEventNotificationHandlerTestsFixtureBase.SenderHashedId}/transfers/connections"
                    && tokens["account_name"] == fixture.ReceiverAccount.Name)),
            Times.Exactly(3));
    }

    [Test]
    public async Task Handle_WhenRejectedTransferConnectionRequestEventIsHandled_ThenShouldSentNotificationWithCorrectProperties()
    {
        var fixture = new RejectedTransferConnectionRequestEventNotificationHandlerTestsFixture();
        await fixture.Handle();

        fixture.NotificationsService.Verify(r => r.SendEmail(
                "TransferConnectionRequestRejected",
                fixture.SenderAccountOwner1.Email,
                It.Is<Dictionary<string, string>>(tokens =>
                    tokens["link_notification_page"] == $"{fixture.Configuration.EmployerFinanceBaseUrl}accounts/{TransferConnectionRequestEventNotificationHandlerTestsFixtureBase.SenderHashedId}/transfers/connections"
                    && tokens["account_name"] == fixture.ReceiverAccount.Name)),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenSentTransferConnectionRequestEventIsHandled_ThenShouldEncodeSenderAccountId()
    {
        var fixture = new RejectedTransferConnectionRequestEventNotificationHandlerTestsFixture();

        await fixture.Handle();

        fixture.EncodingService.Verify(encodingService => encodingService.Encode(It.Is<long>(x => x == fixture.SenderAccount.Id), EncodingType.AccountId), Times.Once);
    }
}

public class RejectedTransferConnectionRequestEventNotificationHandlerTestsFixture
    : TransferConnectionRequestEventNotificationHandlerTestsFixtureBase
{
    public RejectedTransferConnectionRequestEventNotificationHandler Handler { get; set; }

    public RejectedTransferConnectionRequestEvent Event { get; set; }

    public RejectedTransferConnectionRequestEventNotificationHandlerTestsFixture()
    {
        AddSenderAccount();
        AddReceiverAccount();
        SetReceiverAccountOwner();
        SetSenderAccountOwner1();
        SetSenderAccountOwner2();
        SetMessage();

        Handler = new RejectedTransferConnectionRequestEventNotificationHandler(
            Configuration,
            OuterApiClient.Object,
            Mock.Of<ILogger<RejectedTransferConnectionRequestEventNotificationHandler>>(),
            NotificationsService.Object,
            EncodingService.Object);
    }

    public Task Handle()
    {
        return Handler.Handle(Event, null);
    }

    private RejectedTransferConnectionRequestEventNotificationHandlerTestsFixture SetMessage()
    {
        Event = new RejectedTransferConnectionRequestEvent
        {
            ReceiverAccountId = ReceiverAccount.Id,
            ReceiverAccountName = ReceiverAccount.Name,
            SenderAccountId = SenderAccount.Id
        };

        return this;
    }
}