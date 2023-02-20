using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;

public class GetTransferConnectionInvitationAuthorizationQuery : IAuthorizationContextModel, IRequest<GetTransferConnectionInvitationAuthorizationResponse>
{
    [IgnoreMap]
    [Required]
    public long AccountId { get; set; }
}