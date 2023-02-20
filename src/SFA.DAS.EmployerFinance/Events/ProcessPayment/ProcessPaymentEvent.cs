namespace SFA.DAS.EmployerFinance.Events.ProcessPayment;

public class ProcessPaymentEvent : INotification
{
    public long AccountId { get; set; }
}