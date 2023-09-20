using SFA.DAS.EmployerFinance.Events.Messages;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class RejectedTransferConnectionRequestEventHandler : IHandleMessages<RejectedTransferConnectionRequestEvent>
{
    private readonly ILegacyTopicMessagePublisher _messagePublisher;

    public RejectedTransferConnectionRequestEventHandler(ILegacyTopicMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public async Task Handle(RejectedTransferConnectionRequestEvent message, IMessageHandlerContext context)
    {
        await _messagePublisher.PublishAsync(new RejectedTransferConnectionInvitationEvent
        {
            TransferConnectionInvitationId = message.TransferConnectionRequestId,
            SenderAccountId = message.SenderAccountId,
            SenderAccountName = message.SenderAccountName,
            ReceiverAccountId = message.ReceiverAccountId,
            ReceiverAccountHashedId = message.ReceiverAccountHashedId,
            ReceiverAccountName = message.ReceiverAccountName,
            RejectorUserId = message.RejectorUserId,
            RejectorUserExternalId = message.RejectorUserRef,
            RejectorUserName = message.RejectorUserName,
            CreatedAt = message.Created
        });
    }
}