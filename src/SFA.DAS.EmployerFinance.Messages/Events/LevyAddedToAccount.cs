namespace SFA.DAS.EmployerFinance.Messages.Events
{
    public class LevyAddedToAccount
    {
        public long AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}