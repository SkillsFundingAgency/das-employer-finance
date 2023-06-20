namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitation;

public class GetTransferConnectionInvitationQuery : IRequest<GetTransferConnectionInvitationResponse>
{
    public long? TransferConnectionInvitationId { get; set; }

    public long AccountId { get; set; }
}