namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;

public class SentTransferConnectionInvitationViewModel
{
    [Required(ErrorMessage = "Select an option")]
    [RegularExpression("GoToTransfersPage|GoToHomepage", ErrorMessage = "OSelect an option")]
    public string Choice { get; set; }

    public string ReceiverAccountName { get; set; }
    public string SenderAccountName { get; set; }
    public string ReceiverPublicHashedId { get; set; }
        
    public string HashedAccountId { get; set; }
    public string HashedTransferConnectionInvitationId { get; set; }
}