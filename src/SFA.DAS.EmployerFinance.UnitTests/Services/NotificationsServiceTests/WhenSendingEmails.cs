using AutoFixture.NUnit3;
using NServiceBus;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.NotificationsServiceTests;

public class WhenSendingEmails
{
    [Test, MoqAutoData]
    public async Task Then_The_MessageSession_Send_Method_Is_Called(
        string recipientsAddress,
        string templateId,
        IReadOnlyDictionary<string, string> tokens,
        [Frozen] Mock<IMessageSession> publisher,
        NotificationsService service)
    {
        await service.SendEmail(templateId, recipientsAddress, tokens);

        publisher.Verify(x => x.Send(It.Is<SendEmailCommand>(c =>
                c.TemplateId == templateId
                && c.RecipientsAddress == recipientsAddress
                && c.Tokens == tokens),
            It.IsAny<SendOptions>()
        ), Times.Once);
    }
}