namespace SFA.DAS.EmployerFinance.Queries.GetApprovedTransferConnectionInvitation;

public class GetApprovedTransferConnectionInvitationQuery : IRequest<GetApprovedTransferConnectionInvitationResponse>
{
    public long AccountId { get; set; }

    public long? TransferConnectionInvitationId { get; set; }
}