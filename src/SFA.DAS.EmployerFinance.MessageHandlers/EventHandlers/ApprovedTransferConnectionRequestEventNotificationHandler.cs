using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Accounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Accounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class ApprovedTransferConnectionRequestEventNotificationHandler : IHandleMessages<ApprovedTransferConnectionRequestEvent>
{
    private readonly EmployerFinanceConfiguration _config;
    private readonly IOuterApiClient _outerApiClient;
    private readonly ILogger<ApprovedTransferConnectionRequestEventNotificationHandler> _logger;
    private readonly INotificationsService _notificationsService;
    private readonly IEncodingService _encodingService;

    public ApprovedTransferConnectionRequestEventNotificationHandler(
        EmployerFinanceConfiguration config,
        IOuterApiClient outerApiClient,
        ILogger<ApprovedTransferConnectionRequestEventNotificationHandler> logger,
        INotificationsService notificationsService,
        IEncodingService encodingService)
    {
        _config = config;
        _outerApiClient = outerApiClient;
        _logger = logger;
        _notificationsService = notificationsService;
        _encodingService = encodingService;
    }

    public async Task Handle(ApprovedTransferConnectionRequestEvent message, IMessageHandlerContext context)
    {
        var users = await _outerApiClient.Get<GetAccountTeamMembersWhichReceiveNotificationsResponse>(
            new GetAccountTeamMembersWhichReceiveNotificationsRequest(message.SenderAccountId));

        if (users == null)
        {
            throw new InvalidOperationException($"Unable to send approved transfer request notifications for SenderAccountId '{message.SenderAccountId}'");
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

                _logger.LogInformation("{TypeName} linkNotificationUrl: '{LinkNotificationUrl}'", nameof(ApprovedTransferConnectionRequestEventNotificationHandler), linkNotificationUrl);

                await SendNotification(message, user, linkNotificationUrl);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to send approved transfer request notification to UserRef '{UserRef}' for SenderAccountId '{SenderAccountId}'", user.UserRef, message.SenderAccountId);
            }
        }
    }

    private async Task SendNotification(ApprovedTransferConnectionRequestEvent message, TeamMember user, string linkNotificationUrl)
    {
        const string templateId = "TransferConnectionRequestApproved";

        var tokens = new Dictionary<string, string>
        {
            { "name", user.FirstName },
            { "account_name", message.ReceiverAccountName },
            { "link_notification_page", linkNotificationUrl }
        };

        await _notificationsService.SendEmail(templateId, user.Email, tokens);
    }
}