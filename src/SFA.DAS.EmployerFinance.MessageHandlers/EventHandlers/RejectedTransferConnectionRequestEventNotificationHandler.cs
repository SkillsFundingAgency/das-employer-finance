using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Accounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Accounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class RejectedTransferConnectionRequestEventNotificationHandler : IHandleMessages<RejectedTransferConnectionRequestEvent>
{
    private readonly EmployerFinanceConfiguration _config;
    private readonly IOuterApiClient _outerApiClient;
    private readonly ILogger<RejectedTransferConnectionRequestEventNotificationHandler> _logger;
    private readonly INotificationsApi _notificationsApi;
    private readonly IEncodingService _encodingService;

    public RejectedTransferConnectionRequestEventNotificationHandler(
        EmployerFinanceConfiguration config,
        IOuterApiClient outerApiClient,
        ILogger<RejectedTransferConnectionRequestEventNotificationHandler> logger,
        INotificationsApi notificationsApi,
        IEncodingService encodingService)
    {
        _config = config;
        _outerApiClient = outerApiClient;
        _logger = logger;
        _notificationsApi = notificationsApi;
        _encodingService = encodingService;
    }

    public async Task Handle(RejectedTransferConnectionRequestEvent message, IMessageHandlerContext context)
    {
        var users = await _outerApiClient.Get<GetAccountTeamMembersWhichReceiveNotificationsResponse>(
            new GetAccountTeamMembersWhichReceiveNotificationsRequest(message.SenderAccountId));

        if (users == null)
        {
            throw new InvalidOperationException($"Unable to send rejected transfer request notifications for SenderAccountId '{message.SenderAccountId}'");
        }

        if (!users.Any())
        {
            _logger.LogInformation("There are no users that receive notifications for SenderAccountId '{SenderAccountId}'", message.SenderAccountId);
        }
        
        var senderAccountHashedId = _encodingService.Encode(message.SenderAccountId, EncodingType.AccountId);

        foreach (var user in users)
        {
            try
            {
                var linkNotificationUrl = $"{_config.EmployerFinanceBaseUrl}accounts/{senderAccountHashedId}/transfers/connections";
                
                _logger.LogInformation("{TypeName} linkNotificationUrl: '{LinkNotificationUrl}'",
                    nameof(RejectedTransferConnectionRequestEventNotificationHandler), linkNotificationUrl);
                
                var email = new Email
                {
                    RecipientsAddress = user.Email,
                    TemplateId = "TransferConnectionRequestRejected",
                    ReplyToAddress = "noreply@sfa.gov.uk",
                    Subject = "x",
                    SystemId = "x",
                    Tokens = new Dictionary<string, string>
                    {
                        { "name", user.FirstName },
                        { "account_name", message.ReceiverAccountName },
                        { "link_notification_page", linkNotificationUrl }
                    }
                };

                await _notificationsApi.SendEmail(email);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to send rejected transfer request notification to UserRef '{UserRef}' for SenderAccountId '{SenderAccountId}'", user.UserRef, message.SenderAccountId);
            }
        }
    }
}