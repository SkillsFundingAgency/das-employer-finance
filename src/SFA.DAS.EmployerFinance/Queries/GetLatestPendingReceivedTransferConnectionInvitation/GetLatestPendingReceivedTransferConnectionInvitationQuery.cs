using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetLatestPendingReceivedTransferConnectionInvitation;

public class GetLatestPendingReceivedTransferConnectionInvitationQuery : IAuthorizationContextModel, IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>
{
    [IgnoreMap]
    [Required]
    public long AccountId { get; set; }
}