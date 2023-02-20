using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetRejectedTransferConnectionInvitation;

public class GetRejectedTransferConnectionInvitationQuery : IAuthorizationContextModel, IRequest<GetRejectedTransferConnectionInvitationResponse>
{
    [IgnoreMap]
    [Required]
    public long AccountId { get; set; }

    [Required]
    public int? TransferConnectionInvitationId { get; set; }
}