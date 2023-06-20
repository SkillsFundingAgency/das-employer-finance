
namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;

public class GetTransferConnectionInvitationAuthorizationQuery : IRequest<GetTransferConnectionInvitationAuthorizationResponse>
{
    public long AccountId { get; set; }
}