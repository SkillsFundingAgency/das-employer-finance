﻿namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;

public class ApprovedTransferConnectionInvitationViewModel
{
    [Required(ErrorMessage = "Select an option")]
    [RegularExpression("GoToApprenticesPage|GoToHomepage", ErrorMessage = "Select an option")]
    public string Choice { get; set; }
    public string HashedAccountId { get; set; }
    public string HashedTransferConnectionInvitationId { get; set; }
    public string SenderAccountName { get; set; }
}