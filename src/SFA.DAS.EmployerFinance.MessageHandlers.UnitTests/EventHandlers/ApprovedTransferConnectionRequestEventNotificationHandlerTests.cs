﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Notifications.Api.Types;
using SFA.DAS.Testing;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class ApprovedTransferConnectionRequestEventNotificationHandlerTests : FluentTest<ApprovedTransferConnectionRequestEventNotificationHandlerTestFixture>
    {
        [Test]
        public Task Handle_WhenApprovedTransferConnectionRequestEventIsHandled_ThenShouldNotifyAccountOwnersRequiringNotification()
        {
            return RunAsync(f => f.Handle(),
                f => f.NotificationsApiClient.Verify(
                    r => r.SendEmail(It.Is<Email>(e =>
                        !string.IsNullOrWhiteSpace(e.Tokens["link_notification_page"])
                        && e.Tokens["account_name"] == f.ReceiverAccount.Name)),
                    Times.Exactly(3)));
        }

        [Test]
        public Task Handle_WhenApprovedTransferConnectionRequestEventIsHandled_ThenShouldSentNotificationWithCorrectProperties()
        {
            return RunAsync(f => f.Handle(),
                f => f.NotificationsApiClient.Verify(
                    r => r.SendEmail(It.Is<Email>(e =>
                        e.RecipientsAddress == f.SenderAccountOwner1.Email
                        && !string.IsNullOrWhiteSpace(e.Subject)
                        && e.ReplyToAddress == "noreply@sfa.gov.uk"
                        && e.TemplateId == "TransferConnectionRequestApproved"
                        && !string.IsNullOrWhiteSpace(e.Tokens["link_notification_page"])
                        && e.Tokens["account_name"] == f.ReceiverAccount.Name)),
                    Times.Once));
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
                NotificationsApiClient.Object);
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
}