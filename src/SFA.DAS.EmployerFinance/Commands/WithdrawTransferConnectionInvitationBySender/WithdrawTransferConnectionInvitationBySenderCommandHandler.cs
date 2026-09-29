using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Commands.WithdrawTransferConnectionInvitationBySender;

public class WithdrawTransferConnectionInvitationBySenderCommandHandler(
    IEmployerAccountRepository employerAccountRepository,
    ITransferConnectionInvitationRepository transferConnectionInvitationRepository)
    : IRequestHandler<WithdrawTransferConnectionInvitationBySenderCommand>
{
    public async Task Handle(WithdrawTransferConnectionInvitationBySenderCommand request, CancellationToken cancellationToken)
    {
        var senderAccount = await employerAccountRepository.Get(request.SenderAccountId);
        if (senderAccount == null)
        {
            throw new InvalidOperationException(
                $"Sender account {request.SenderAccountId} was not found");
        }

        var transferConnectionInvitation = await transferConnectionInvitationRepository.GetBySender(
            request.TransferConnectionInvitationId,
            request.SenderAccountId,
            TransferConnectionInvitationStatus.Pending);

        if (transferConnectionInvitation == null)
        {
            throw new InvalidOperationException(
                $"Pending transfer connection invitation {request.TransferConnectionInvitationId} was not found for sender account {request.SenderAccountId}");
        }

        var actingUser = GetActingUser(transferConnectionInvitation);
        transferConnectionInvitation.WithdrawBySender(senderAccount, actingUser);
    }

    private static User GetActingUser(TransferConnectionInvitation invitation)
    {
        var actingUser = invitation.Changes
            .OrderBy(c => c.CreatedDate)
            .Select(c => c.User)
            .FirstOrDefault(u => u != null);

        if (actingUser == null)
        {
            throw new InvalidOperationException(
                $"Pending transfer connection invitation {invitation.Id} has no associated user change to attribute the system withdrawal to");
        }

        return actingUser;
    }
}
