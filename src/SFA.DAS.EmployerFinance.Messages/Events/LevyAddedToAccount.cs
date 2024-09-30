namespace SFA.DAS.EmployerFinance.Messages.Events;

public class LevyAddedToAccount : Message
{
    public long AccountId { get; set; }
    public decimal Amount { get; set; }
}