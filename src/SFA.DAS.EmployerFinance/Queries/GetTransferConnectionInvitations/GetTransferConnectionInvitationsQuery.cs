using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;

public class GetTransferConnectionInvitationsQuery : IAuthorizationContextModel, IRequest<GetTransferConnectionInvitationsResponse>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }
}