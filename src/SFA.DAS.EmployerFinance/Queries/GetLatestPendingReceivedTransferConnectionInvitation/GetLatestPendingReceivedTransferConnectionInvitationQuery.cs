namespace SFA.DAS.EmployerFinance.Queries.GetLatestPendingReceivedTransferConnectionInvitation;

public class GetLatestPendingReceivedTransferConnectionInvitationQuery :  IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>
{
    public long AccountId { get; set; }
}