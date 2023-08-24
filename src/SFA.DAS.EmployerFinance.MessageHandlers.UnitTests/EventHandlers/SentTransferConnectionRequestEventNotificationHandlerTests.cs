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
public class SentTransferConnectionRequestEventNotificationHandlerTests 
{
    [Test]
    public async Task Handle_WhenSentTransferConnectionRequestEventIsHandled_ThenShouldNotifyAccountOwnersRequiringNotification()
    {
        var fixture = new SentTransferConnectionRequestEventNotificationHandlerTestsFixture();
        
        await fixture.Handle();

        fixture.NotificationsApiClient.Verify(
            api => api.SendEmail(It.Is<Email>(email =>
                !string.IsNullOrWhiteSpace(email.Tokens["link_notification_page"])
                && email.Tokens["account_name"] == fixture.SenderAccount.Name)),
            Times.Exactly(3));
    }
    
    [Test]
    public async Task Handle_WhenSentTransferConnectionRequestEventIsHandled_ThenShouldEncodeReceiverAccountId()
    {
        var fixture = new SentTransferConnectionRequestEventNotificationHandlerTestsFixture();
        
        await fixture.Handle();
        
        fixture.EncodingService.Verify(encodingService=> encodingService.Encode(It.Is<long>(x=> x == fixture.ReceiverAccount.Id), EncodingType.AccountId), Times.Once);
    }
    
    [Test]
    public async Task Handle_WhenSentTransferConnectionRequestEventIsHandled_ThenShouldSentNotificationWithCorrectProperties()
    {
        var fixture = new SentTransferConnectionRequestEventNotificationHandlerTestsFixture();
        
        await fixture.Handle();
        
        fixture.NotificationsApiClient.Verify(
            api => api.SendEmail(It.Is<Email>(email =>
                email.RecipientsAddress == fixture.ReceiverAccountOwner.Email
                && !string.IsNullOrWhiteSpace(email.Subject)
                && email.ReplyToAddress == "noreply@sfa.gov.uk"
                && email.TemplateId == "TransferConnectionInvitationSent"
                && email.Tokens["link_notification_page"] ==  $"{fixture.Configuration.EmployerFinanceBaseUrl}accounts/{fixture.ReceiverHashedId}/transfers/connections"
                && email.Tokens["account_name"] == fixture.SenderAccount.Name)),
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
            NotificationsApiClient.Object,
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