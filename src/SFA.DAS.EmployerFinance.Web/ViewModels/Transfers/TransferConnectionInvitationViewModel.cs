using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Web.Attributes;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class TransferConnectionInvitationViewModel : IAuthorizationContextModel
    {
        [Required(ErrorMessage = "Option required")]
        [RegularExpression("Confirm|GoToTransfersPage", ErrorMessage = "Option required")]
        public string Choice { get; set; }

        [Required]
        public int? TransferConnectionInvitationId { get; set; }

        public TransferConnectionInvitationDto TransferConnectionInvitation { get; set; }
        public string HashedAccountId { get; set; }
        public string HashedTransferConnectionInvitationId { get; set; }
    }
}