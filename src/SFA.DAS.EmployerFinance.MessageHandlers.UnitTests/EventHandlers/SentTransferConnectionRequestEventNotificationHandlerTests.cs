﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.EventHandlers;

[TestFixture]
public class SentTransferConnectionRequestEventNotificationHandlerTests
{
    [Test]
    public async Task Handle_WhenSentTransferConnectionRequestEventIsHandled_ThenShouldNotifyAccountOwnersRequiringNotification()
    {
        var fixture = new SentTransferConnectionRequestEventNotificationHandlerTestsFixture();

        await fixture.Handle();

        fixture.NotificationsService.Verify(
            api => api.SendEmail(
                "TransferConnectionInvitationSent",
                It.IsAny<string>(),
                It.Is<Dictionary<string, string>>(tokens =>
                    !string.IsNullOrWhiteSpace(tokens["link_notification_page"])
                    && tokens["account_name"] == fixture.SenderAccount.Name)),
            Times.Exactly(3));
    }

    [Test]
    public async Task Handle_WhenSentTransferConnectionRequestEventIsHandled_ThenShouldEncodeReceiverAccountId()
    {
        var fixture = new SentTransferConnectionRequestEventNotificationHandlerTestsFixture();

        await fixture.Handle();

        fixture.EncodingService.Verify(encodingService => encodingService.Encode(It.Is<long>(x => x == fixture.ReceiverAccount.Id), EncodingType.AccountId), Times.Once);
    }

    [Test]
    public async Task Handle_WhenSentTransferConnectionRequestEventIsHandled_ThenShouldSentNotificationWithCorrectProperties()
    {
        var fixture = new SentTransferConnectionRequestEventNotificationHandlerTestsFixture();

        await fixture.Handle();

        fixture.NotificationsService.Verify(api => api.SendEmail(
                "TransferConnectionInvitationSent",
                fixture.ReceiverAccountOwner.Email,
                It.Is<Dictionary<string, string>>(tokens =>
                    tokens["link_notification_page"] == $"{fixture.Configuration.EmployerFinanceBaseUrl}accounts/{TransferConnectionRequestEventNotificationHandlerTestsFixtureBase.ReceiverHashedId}/transfers/connections"
                    && tokens["account_name"] == fixture.SenderAccount.Name)),
            Times.Once);
    }
}

public class SentTransferConnectionRequestEventNotificationHandlerTestsFixture : TransferConnectionRequestEventNotificationHandlerTestsFixtureBase
{
    private SentTransferConnectionRequestEventNotificationHandler Handler { get; }

    private SentTransferConnectionRequestEvent Event { get; set; }

    public SentTransferConnectionRequestEventNotificationHandlerTestsFixture()
    {
        AddSenderAccount();
        AddReceiverAccount();
        SetReceiverAccountOwner();
        SetSenderAccountOwner1();
        SetSenderAccountOwner2();
        SetMessage();

        Handler = new SentTransferConnectionRequestEventNotificationHandler(
            Configuration,
            OuterApiClient.Object,
            Mock.Of<ILogger<SentTransferConnectionRequestEventNotificationHandler>>(),
            NotificationsService.Object,
            EncodingService.Object);
    }

    public Task Handle()
    {
        return Handler.Handle(Event, null);
    }

    private void SetMessage()
    {
        Event = new SentTransferConnectionRequestEvent
        {
            ReceiverAccountId = ReceiverAccount.Id,
            SenderAccountId = SenderAccount.Id,
            SenderAccountName = SenderAccount.Name
        };
    }
}