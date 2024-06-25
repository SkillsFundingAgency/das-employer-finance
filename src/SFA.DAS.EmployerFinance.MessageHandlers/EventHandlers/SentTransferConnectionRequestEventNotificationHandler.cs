using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Accounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Accounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class SentTransferConnectionRequestEventNotificationHandler : IHandleMessages<SentTransferConnectionRequestEvent>
{
    private readonly EmployerFinanceConfiguration _config;
    private readonly IOuterApiClient _outerApiClient;
    private readonly ILogger<SentTransferConnectionRequestEventNotificationHandler> _logger;
    private readonly INotificationsService _notificationsService;
    private readonly IEncodingService _encodingService;

    public SentTransferConnectionRequestEventNotificationHandler(
        EmployerFinanceConfiguration config,
        IOuterApiClient outerApiClient,
        ILogger<SentTransferConnectionRequestEventNotificationHandler> logger,
        INotificationsService notificationsService,
        IEncodingService encodingService)
    {
        _config = config;
        _outerApiClient = outerApiClient;
        _logger = logger;
        _notificationsService = notificationsService;
        _encodingService = encodingService;
    }

    public async Task Handle(SentTransferConnectionRequestEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation("{TypeName} processing started.", nameof(SentTransferConnectionRequestEventNotificationHandler));

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

        var receiverHashedId = _encodingService.Encode(message.ReceiverAccountId, EncodingType.AccountId);

        foreach (var user in users)
        {
            try
            {
                var linkNotificationUrl = $"{_config.EmployerFinanceBaseUrl}accounts/{receiverHashedId}/transfers/connections";

                _logger.LogInformation("{TypeName} linkNotificationUrl: '{LinkNotificationUrl}'", nameof(SentTransferConnectionRequestEventNotificationHandler), linkNotificationUrl);

                await SendNotification(message, user, linkNotificationUrl);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to send sent transfer request notification to UserRef '{UserRef}' for ReceiverAccountId '{ReceiverAccountId}'", user.UserRef, message.ReceiverAccountId);
            }
        }

        _logger.LogInformation("{TypeName} processing completed", nameof(SentTransferConnectionRequestEventNotificationHandler));
    }

    private async Task SendNotification(SentTransferConnectionRequestEvent message, TeamMember user, string linkNotificationUrl)
    {
        const string templateId = "TransferConnectionInvitationSent";

        var tokens = new Dictionary<string, string>
        {
            { "name", user.FirstName },
            { "account_name", message.SenderAccountName },
            { "link_notification_page", linkNotificationUrl }
        };

        await _notificationsService.SendEmail(templateId, user.Email, tokens);
    }
}