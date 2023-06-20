namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;

public class RejectedTransferConnectionInvitationViewModel
{
    [Required(ErrorMessage = "Select an option")]
    [RegularExpression("Confirm|GoToTransfersPage", ErrorMessage = "Select an option")]
    public string Choice { get; set; }

    public string HashedAccountId { get; set; }
    public string HashedTransferConnectionInvitationId { get; set; }
    public string Status { get; set; }
    public string SenderAccountName { get; set; }
    public string PendingUserName { get; set; }
    public DateTime PendingChangeCreatedDate { get; set; }
    public string ReceiverAccountName { get; set; }
    public string ReceiverAccountPublicHashedId { get; set; }
}