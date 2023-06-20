namespace SFA.DAS.EmployerFinance.Queries.GetReceivedTransferConnectionInvitation;

public class GetReceivedTransferConnectionInvitationQuery : IRequest<GetReceivedTransferConnectionInvitationResponse>
{
    public long AccountId { get; set; }

    public long? TransferConnectionInvitationId { get; set; }
}