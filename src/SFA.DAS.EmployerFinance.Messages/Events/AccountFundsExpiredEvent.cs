namespace SFA.DAS.EmployerFinance.Messages.Events;

public class AccountFundsExpiredEvent : Message
{
    public long AccountId { get; set; }
}