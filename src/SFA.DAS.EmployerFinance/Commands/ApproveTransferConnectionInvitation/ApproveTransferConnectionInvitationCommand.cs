namespace SFA.DAS.EmployerFinance.Commands.ApproveTransferConnectionInvitation;

public class ApproveTransferConnectionInvitationCommand : IRequest
{
    
    public long AccountId { get; set; }

    public Guid UserRef { get; set; }

    public int? TransferConnectionInvitationId { get; set; }
}