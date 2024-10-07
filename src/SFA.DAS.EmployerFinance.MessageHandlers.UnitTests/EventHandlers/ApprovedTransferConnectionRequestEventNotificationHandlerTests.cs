using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class ApprovedTransferConnectionRequestEventNotificationHandlerTests
{
    [Test]
    public async Task Handle_WhenApprovedTransferConnectionRequestEventIsHandled_ThenShouldNotifyAccountOwnersRequiringNotification()
    {
        var fixture = new ApprovedTransferConnectionRequestEventNotificationHandlerTestFixture();

        await fixture.Handle();

        fixture.NotificationsService.Verify(r => r.SendEmail(
                "TransferConnectionRequestApproved",
                It.IsAny<string>(),
                It.Is<Dictionary<string, string>>(tokens =>
                    tokens["link_notification_page"] == $"{fixture.Configuration.EmployerFinanceBaseUrl}accounts/{TransferConnectionRequestEventNotificationHandlerTestsFixtureBase.SenderHashedId}/transfers/connections"
                    && tokens["account_name"] == fixture.ReceiverAccount.Name
                    && tokens["name"] != string.Empty
                )),
            Times.Exactly(3));
    }

    [Test]
    public async Task Handle_WhenApprovedTransferConnectionRequestEventIsHandled_ThenShouldSentNotificationWithCorrectProperties()
    {
        var fixture = new ApprovedTransferConnectionRequestEventNotificationHandlerTestFixture();

        await fixture.Handle();

        fixture.NotificationsService.Verify(r => r.SendEmail(
                "TransferConnectionRequestApproved",
                fixture.SenderAccountOwner1.Email,
                It.Is<Dictionary<string, string>>(tokens =>
                    tokens["link_notification_page"] == $"{fixture.Configuration.EmployerFinanceBaseUrl}accounts/{TransferConnectionRequestEventNotificationHandlerTestsFixtureBase.SenderHashedId}/transfers/connections"
                    && tokens["account_name"] == fixture.ReceiverAccount.Name
                    && tokens["name"] != string.Empty
                )),
            Times.Once);
    }

    [Test]
    public async Task Handle_WhenSentTransferConnectionRequestEventIsHandled_ThenShouldEncodeSenderAccountId()
    {
        var fixture = new ApprovedTransferConnectionRequestEventNotificationHandlerTestFixture();

        await fixture.Handle();

        fixture.EncodingService.Verify(encodingService => encodingService.Encode(It.Is<long>(x => x == fixture.SenderAccount.Id), EncodingType.AccountId), Times.Once);
    }
}

public class ApprovedTransferConnectionRequestEventNotificationHandlerTestFixture
    : TransferConnectionRequestEventNotificationHandlerTestsFixtureBase
{
    public ApprovedTransferConnectionRequestEventNotificationHandler Handler { get; set; }

    public ApprovedTransferConnectionRequestEvent Event { get; set; }

    public ApprovedTransferConnectionRequestEventNotificationHandlerTestFixture()
    {
        AddSenderAccount();
        AddReceiverAccount();
        SetReceiverAccountOwner();
        SetSenderAccountOwner1();
        SetSenderAccountOwner2();
        SetMessage();

        Handler = new ApprovedTransferConnectionRequestEventNotificationHandler(
            Configuration,
            OuterApiClient.Object,
            Mock.Of<ILogger<ApprovedTransferConnectionRequestEventNotificationHandler>>(),
            NotificationsService.Object,
            EncodingService.Object);
    }

    public Task Handle()
    {
        return Handler.Handle(Event, null);
    }

    private ApprovedTransferConnectionRequestEventNotificationHandlerTestFixture SetMessage()
    {
        Event = new ApprovedTransferConnectionRequestEvent
        {
            ReceiverAccountId = ReceiverAccount.Id,
            ReceiverAccountName = ReceiverAccount.Name,
            SenderAccountId = SenderAccount.Id
        };

        return this;
    }
}