using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerFinance.Commands.WithdrawTransferConnectionInvitationBySender;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class ApprenticeshipEmployerTypeChangeEventHandler(
    ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
    IMediator mediator,
    ILogger<ApprenticeshipEmployerTypeChangeEventHandler> logger)
    : IHandleMessages<ApprenticeshipEmployerTypeChangeEvent>
{
    public async Task Handle(ApprenticeshipEmployerTypeChangeEvent message, IMessageHandlerContext context)
    {
        if (message.ApprenticeshipEmployerType != ApprenticeshipEmployerType.NonLevy)
        {
            logger.LogInformation(
                "Ignoring ApprenticeshipEmployerTypeChangeEvent for account {AccountId} because employer type is {EmployerType}",
                message.AccountId,
                message.ApprenticeshipEmployerType);
            return;
        }

        logger.LogInformation("Handling ApprenticeshipEmployerTypeChangeEvent for account {AccountId}", message.AccountId);

        var pendingInvitations = await transferConnectionInvitationRepository.GetPendingBySender(message.AccountId);

        foreach (var invitation in pendingInvitations)
        {
            await mediator.Send(new WithdrawTransferConnectionInvitationBySenderCommand
            {
                TransferConnectionInvitationId = invitation.Id,
                SenderAccountId = message.AccountId
            });
        }

        logger.LogInformation(
            "Processed {InvitationCount} pending transfer connection withdrawals for account {AccountId}",
            pendingInvitations.Count,
            message.AccountId);
    }
}
