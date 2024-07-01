using NServiceBus;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.EmployerFinance.Services;

public interface INotificationsService
{
    Task SendEmail(string templateId, string recipientsAddress, IReadOnlyDictionary<string, string> tokens);
}

public class NotificationsService(IMessageSession publisher) : INotificationsService
{
    public async Task SendEmail(string templateId, string recipientsAddress, IReadOnlyDictionary<string, string> tokens)
    {
        var message = new SendEmailCommand(
            templateId,
            recipientsAddress,
            tokens
        );

        await publisher.Send(message);
    }
}