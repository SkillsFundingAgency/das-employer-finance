using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;

public class TransferConnectionInvitationsViewModel
{
    public long AccountId { get; set; }

    public IEnumerable<TransferConnectionInvitationDto> TransferConnectionInvitations { get; set; }

    public IEnumerable<TransferConnectionInvitationDto> TransferSenderConnectionInvitations { get; set; }
    
    public IEnumerable<TransferConnectionInvitationDto> TransferReceiverConnectionInvitations { get; set; } 
    
    public string HashedAccountId { get; set; }
}