using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;

public class GetTransferConnectionInvitationAuthorizationQuery : IRequest<GetTransferConnectionInvitationAuthorizationResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }
}