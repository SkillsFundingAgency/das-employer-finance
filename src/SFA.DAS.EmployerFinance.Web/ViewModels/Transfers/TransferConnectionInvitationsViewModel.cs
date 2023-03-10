using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class TransferConnectionInvitationsViewModel
    {
        public long AccountId { get; set; }

        public IEnumerable<TransferConnectionInvitationDto> TransferConnectionInvitations { get; set; }

        public IEnumerable<TransferConnectionInvitationDto> TransferSenderConnectionInvitations { get; set; }
            //=> TransferConnectionInvitations.Where(p => p.SenderAccount.Id == AccountId);

        public IEnumerable<TransferConnectionInvitationDto> TransferReceiverConnectionInvitations { get; set; } 
            //=> TransferConnectionInvitations.Where(p => p.ReceiverAccount.Id == AccountId);

        public string HashedAccountId { get; set; }
    }
}