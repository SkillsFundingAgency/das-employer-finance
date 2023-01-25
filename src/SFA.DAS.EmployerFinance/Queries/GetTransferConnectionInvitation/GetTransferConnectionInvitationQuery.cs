using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitation
{
    public class GetTransferConnectionInvitationQuery : IAuthorizationContextModel, IRequest<GetTransferConnectionInvitationResponse>
    {
        [IgnoreMap]
        [Required]
        public long AccountId { get; set; }

        [Required]
        public int? TransferConnectionInvitationId { get; set; }
    }
}