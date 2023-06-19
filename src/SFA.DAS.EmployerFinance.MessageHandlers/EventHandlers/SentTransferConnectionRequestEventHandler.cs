using SFA.DAS.EmployerFinance.Events.Messages;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class SentTransferConnectionRequestEventHandler : IHandleMessages<SentTransferConnectionRequestEvent>
{
    private readonly ILegacyTopicMessagePublisher _messagePublisher;
    private readonly ILogger<SentTransferConnectionRequestEventHandler> _logger;

    public SentTransferConnectionRequestEventHandler(ILegacyTopicMessagePublisher messagePublisher, ILogger<SentTransferConnectionRequestEventHandler> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task Handle(SentTransferConnectionRequestEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation($"Starting {nameof(SentTransferConnectionRequestEventHandler)}.");

        await _messagePublisher.PublishAsync(new SentTransferConnectionInvitationEvent
        {
            TransferConnectionInvitationId = message.TransferConnectionRequestId,
            SenderAccountId = message.SenderAccountId,
            SenderAccountHashedId = message.SenderAccountHashedId,
            SenderAccountName = message.SenderAccountName,
            ReceiverAccountId = message.ReceiverAccountId,
            ReceiverAccountHashedId = message.ReceiverAccountHashedId,
            ReceiverAccountName = message.ReceiverAccountName,
            SentByUserId = message.SentByUserId,
            SentByUserExternalId = message.SentByUserRef,
            SentByUserName = message.SentByUserName,
            CreatedAt = message.Created
        });

        _logger.LogInformation($"Completed {nameof(SentTransferConnectionRequestEventHandler)}.");
    }
}