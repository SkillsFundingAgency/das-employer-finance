using System;

namespace SFA.DAS.EmployerFinance.Messages.Commands
{
    public class DraftExpireAccountFundsCommand
    {
        public long AccountId { get; set; }
        public DateTime? DateTo { get; set; }
    }
}