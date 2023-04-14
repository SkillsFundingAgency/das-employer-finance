using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers
{
    public class TransferConnectionInvitationViewModel
    {
        [Required(ErrorMessage = "Select an option")]
        [RegularExpression("Confirm|GoToTransfersPage", ErrorMessage = "Select an option")]
        public string Choice { get; set; }

        public int? Id { get; set; }

        public TransferConnectionInvitationDto TransferConnectionInvitation { get; set; }
        public string HashedAccountId { get; set; }
        public string HashedTransferConnectionInvitationId { get; set; }
    }
}