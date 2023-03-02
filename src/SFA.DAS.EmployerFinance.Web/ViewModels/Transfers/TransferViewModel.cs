using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class TransferViewModel
{
    public TransferAllowanceViewModel TransferAllowanceViewModel { get; set; }
    public TransferConnectionInvitationAuthorizationViewModel TransferConnectionInvitationAuthorizationViewModel { get; set; }
    public TransferConnectionInvitationsViewModel TransferConnectionInvitationsViewModel { get; set; }
    public TransferConnectionInvitationViewModel TransferConnectionInvitationViewModel { get; set; }
    public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }
    public TransferRequestsViewModel TransferRequest { get; set; }
}