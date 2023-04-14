using SFA.DAS.EmployerFinance.Web.Attributes;

namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers
{
    public class ReceiveTransferConnectionInvitationViewModel
    {
        [IgnoreMap]
        public string TransferConnectionInvitationId { get; set; }

        [Required(ErrorMessage = "Select an option")]
        [RegularExpression("Approve|Reject", ErrorMessage = "Select an option")]
        public string Choice { get; set; }

        [IgnoreMap]
        public string HashedAccountId { get; set; }
        [IgnoreMap]
        public int NotHashedTransferConnectionInvitationId { get; set; }

        public string SenderAccountName { get; set; }
        public string PendingChangeUserFullName { get; set; }
        public DateTime PendingChangeCreatedDate { get; set; }
    }
}