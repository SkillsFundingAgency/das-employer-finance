using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetRejectedTransferConnectionInvitation;

public class GetRejectedTransferConnectionInvitationQuery : IAuthorizationContextModel, IRequest<GetRejectedTransferConnectionInvitationResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }

    [Required]
    public int? TransferConnectionInvitationId { get; set; }
}