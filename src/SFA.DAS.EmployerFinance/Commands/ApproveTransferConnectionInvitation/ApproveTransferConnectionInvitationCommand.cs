using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.ApproveTransferConnectionInvitation;

public class ApproveTransferConnectionInvitationCommand : IAuthorizationContextModel, IRequest<Unit>
{
    [Ignore]
    [Required]
    public long AccountId { get; set; }

    [Ignore]
    [Required]
    public Guid UserRef { get; set; }

    [Required]
    public int? TransferConnectionInvitationId { get; set; }
}