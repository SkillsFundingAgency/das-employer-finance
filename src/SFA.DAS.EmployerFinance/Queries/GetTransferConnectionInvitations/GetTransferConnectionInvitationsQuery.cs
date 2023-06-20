
namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;

public class GetTransferConnectionInvitationsQuery : IRequest<GetTransferConnectionInvitationsResponse>
{
    public long AccountId { get; set; }
}