using System;

namespace SFA.DAS.EmployerFinance.Messages.Events
{
    public class AccountFundsExpiredEvent
    {
        public long AccountId { get; set; }
        public DateTime Created { get; set; }
    }
}
