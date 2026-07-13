using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Commands.WithdrawTransferConnectionInvitationBySender;

public class WithdrawTransferConnectionInvitationBySenderCommandHandler(
    IEmployerAccountRepository employerAccountRepository,
    ITransferConnectionInvitationRepository transferConnectionInvitationRepository)
    : IRequestHandler<WithdrawTransferConnectionInvitationBySenderCommand>
{
    private static readonly User SystemUser = new()
    {
        Id = 0,
        Ref = Guid.Empty,
        FirstName = "System",
        LastName = string.Empty
    };

    public async Task Handle(WithdrawTransferConnectionInvitationBySenderCommand request, CancellationToken cancellationToken)
    {
        var senderAccount = await employerAccountRepository.Get(request.SenderAccountId);
        var transferConnectionInvitation = await transferConnectionInvitationRepository.GetBySender(
            request.TransferConnectionInvitationId,
            request.SenderAccountId,
            TransferConnectionInvitationStatus.Pending);

        if (transferConnectionInvitation == null)
        {
            throw new InvalidOperationException(
                $"Pending transfer connection invitation {request.TransferConnectionInvitationId} was not found for sender account {request.SenderAccountId}");
        }

        transferConnectionInvitation.WithdrawBySender(senderAccount, SystemUser);
    }
}
