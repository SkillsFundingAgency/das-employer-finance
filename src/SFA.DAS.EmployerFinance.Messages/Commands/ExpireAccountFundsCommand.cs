namespace SFA.DAS.EmployerFinance.Messages.Commands;

public class ExpireAccountFundsCommand : Message
{
    public long AccountId { get; set; }
}