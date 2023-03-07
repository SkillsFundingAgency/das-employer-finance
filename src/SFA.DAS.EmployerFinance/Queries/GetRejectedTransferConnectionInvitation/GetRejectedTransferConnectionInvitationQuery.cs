namespace SFA.DAS.EmployerFinance.Queries.GetRejectedTransferConnectionInvitation;

public class GetRejectedTransferConnectionInvitationQuery : IRequest<GetRejectedTransferConnectionInvitationResponse>
{
    
    public long AccountId { get; set; }

    public long? TransferConnectionInvitationId { get; set; }
}