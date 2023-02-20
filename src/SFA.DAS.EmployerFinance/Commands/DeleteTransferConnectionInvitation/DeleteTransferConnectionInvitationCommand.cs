using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.DeleteTransferConnectionInvitation;

public class DeleteTransferConnectionInvitationCommand : IAuthorizationContextModel, IRequest<Unit>
{
    [IgnoreMap]
    [Required]
    public long AccountId { get; set; }

    [IgnoreMap]
    [Required]
    public Guid UserRef { get; set; }

    [Required]
    public int? TransferConnectionInvitationId { get; set; }
}