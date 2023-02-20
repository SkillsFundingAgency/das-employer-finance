using System.ComponentModel.DataAnnotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetReceivedTransferConnectionInvitation;

public class GetReceivedTransferConnectionInvitationQuery : IAuthorizationContextModel, IRequest<GetReceivedTransferConnectionInvitationResponse>
{
    [Required]
    public long AccountId { get; set; }

    [Required]
    public int? TransferConnectionInvitationId { get; set; }
}