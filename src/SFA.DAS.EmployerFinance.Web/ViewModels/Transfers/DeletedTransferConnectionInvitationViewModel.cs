namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;

public class DeletedTransferConnectionInvitationViewModel
{
    [Required(ErrorMessage = "Select an option")]
    [RegularExpression("GoToTransfersPage|GoToHomepage", ErrorMessage = "Select an option")]
    public string Choice { get; set; }

    public string HashedAccountId { get; set; }
    public string HashedTransferConnectionInvitationId { get; set; }
}