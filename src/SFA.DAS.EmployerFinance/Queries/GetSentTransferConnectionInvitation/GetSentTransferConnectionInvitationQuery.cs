using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetSentTransferConnectionInvitation;

public class GetSentTransferConnectionInvitationQuery : IAuthorizationContextModel, IRequest<GetSentTransferConnectionInvitationResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }

    [Required]
    public int? TransferConnectionInvitationId { get; set; }
}