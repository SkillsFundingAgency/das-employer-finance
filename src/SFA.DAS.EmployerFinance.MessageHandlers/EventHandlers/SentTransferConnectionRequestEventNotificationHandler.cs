using System.Text.Json.Nodes;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Accounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Accounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class SentTransferConnectionRequestEventNotificationHandler : IHandleMessages<SentTransferConnectionRequestEvent>
{
    private readonly EmployerFinanceConfiguration _config;
    private readonly IOuterApiClient _outerApiClient;
    private readonly ILogger<SentTransferConnectionRequestEventNotificationHandler> _logger;
    private readonly INotificationsApi _notificationsApi;

    public SentTransferConnectionRequestEventNotificationHandler(
        EmployerFinanceConfiguration config,
        IOuterApiClient outerApiClient,
        ILogger<SentTransferConnectionRequestEventNotificationHandler> logger,
        INotificationsApi notificationsApi)
    {
        _config = config;
        _outerApiClient = outerApiClient;
        _logger = logger;
        _notificationsApi = notificationsApi;
    }

    public async Task Handle(SentTransferConnectionRequestEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation("{TypeName} processing started for message: {Message}", nameof(SentTransferConnectionRequestEventNotificationHandler), System.Text.Json.JsonSerializer.Serialize(message));
        
        ArgumentNullException.ThrowIfNull(message.ReceiverAccountHashedId);
        
        var users = await _outerApiClient.Get<GetAccountTeamMembersWhichReceiveNotificationsResponse>(
            new GetAccountTeamMembersWhichReceiveNotificationsRequest(message.ReceiverAccountId));

        if (users == null)
        {
            throw new InvalidOperationException($"Unable to send sent transfer request notifications for SenderAccountId '{message.SenderAccountId}'");
        }

        if (!users.Any())
        {
            _logger.LogInformation("There are no users that receive notifications for ReceiverAccountId '{ReceiverAccountId}'", message.ReceiverAccountId);
        }

        foreach (var user in users)
        {
            try
            {
                var linkNotificationUrl = $"{_config.EmployerFinanceBaseUrl}accounts/{message.ReceiverAccountHashedId}/transfers/connections";
                
                _logger.LogInformation("{TypeName} linkNotificationUrl: '{LinkNotificationUrl}'",
                    nameof(SentTransferConnectionRequestEventNotificationHandler), linkNotificationUrl);
                
                var email = new Email
                {
                    RecipientsAddress = user.Email,
                    TemplateId = "TransferConnectionInvitationSent",
                    ReplyToAddress = "noreply@sfa.gov.uk",
                    Subject = "x",
                    SystemId = "x",
                    Tokens = new Dictionary<string, string>
                    {
                        { "name", user.FirstName },
                        { "account_name", message.SenderAccountName },
                        { "link_notification_page", linkNotificationUrl }
                    }
                };

                await _notificationsApi.SendEmail(email);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to send sent transfer request notification to UserRef '{UserRef}' for ReceiverAccountId '{ReceiverAccountId}'", user.UserRef,message.ReceiverAccountId);
            }
        }

        _logger.LogInformation("{TypeName} processing completed", nameof(SentTransferConnectionRequestEventNotificationHandler));
    }
}