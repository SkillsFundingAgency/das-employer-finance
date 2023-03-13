using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class RejectedTransferConnectionInvitationViewModel
    {
        [Required(ErrorMessage = "Select an option")]
        [RegularExpression("Confirm|GoToTransfersPage", ErrorMessage = "Select an option")]
        public string Choice { get; set; }

        public TransferConnectionInvitationDto TransferConnectionInvitation { get; set; }
        public string HashedAccountId { get; set; }
        public string HashedTransferConnectionInvitationId { get; set; }
    }
}