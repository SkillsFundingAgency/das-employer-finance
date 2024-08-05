namespace SFA.DAS.EmployerFinance.Commands.DeleteTransferConnectionInvitation;

public class DeleteTransferConnectionInvitationCommand : IRequest
{
    public long AccountId { get; set; }

    public Guid UserRef { get; set; }

    public long? TransferConnectionInvitationId { get; set; }
}