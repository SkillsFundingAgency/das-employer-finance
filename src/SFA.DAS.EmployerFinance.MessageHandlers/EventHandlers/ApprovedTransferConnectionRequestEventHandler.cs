using SFA.DAS.EmployerFinance.Events.Messages;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class ApprovedTransferConnectionRequestEventHandler : IHandleMessages<ApprovedTransferConnectionRequestEvent>
{
    private readonly ILegacyTopicMessagePublisher _messagePublisher;

    public ApprovedTransferConnectionRequestEventHandler(ILegacyTopicMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public async Task Handle(ApprovedTransferConnectionRequestEvent message, IMessageHandlerContext context)
    {
        await _messagePublisher.PublishAsync(new ApprovedTransferConnectionInvitationEvent
        {
            TransferConnectionInvitationId = message.TransferConnectionRequestId,
            SenderAccountId = message.SenderAccountId,
            SenderAccountName = message.SenderAccountName,
            ReceiverAccountId = message.ReceiverAccountId,
            ReceiverAccountName = message.ReceiverAccountName,
            ApprovedByUserId = message.ApprovedByUserId,
            ApprovedByUserExternalId = message.ApprovedByUserRef,
            ApprovedByUserName = message.ApprovedByUserName,
            CreatedAt = message.Created
        });
    }
}