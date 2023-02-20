using System.ComponentModel.DataAnnotations;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.SendTransferConnectionInvitation;

public class SendTransferConnectionInvitationCommand : IAuthorizationContextModel, IRequest<int>
{
    [Required]
    public long AccountId { get; set; }

    [Required]
    public Guid UserRef { get; set; }

    [Required]
    [RegularExpression(Constants.AccountHashedIdRegex)]
    public string ReceiverAccountPublicHashedId { get; set; }
}