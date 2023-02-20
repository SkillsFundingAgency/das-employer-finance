using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.SendTransferConnectionInvitation;

public class SendTransferConnectionInvitationQuery : IAuthorizationContextModel, IRequest<SendTransferConnectionInvitationResponse>
{
    [IgnoreMap]
    [Required]
    public long AccountId { get; set; }

    [Required(ErrorMessage = "You must enter a valid account ID")]
    [RegularExpression(Constants.AccountHashedIdRegex, ErrorMessage = "You must enter a valid account ID")]
    public string ReceiverAccountPublicHashedId { get; set; }
}