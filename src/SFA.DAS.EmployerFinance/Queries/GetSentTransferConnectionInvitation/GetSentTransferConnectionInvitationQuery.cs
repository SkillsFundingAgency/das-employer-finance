namespace SFA.DAS.EmployerFinance.Queries.GetSentTransferConnectionInvitation;

public class GetSentTransferConnectionInvitationQuery : IRequest<GetSentTransferConnectionInvitationResponse>
{
    public long AccountId { get; set; }

    public long? TransferConnectionInvitationId { get; set; }
}