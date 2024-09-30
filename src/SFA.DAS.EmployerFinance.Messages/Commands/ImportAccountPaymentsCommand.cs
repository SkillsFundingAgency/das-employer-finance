namespace SFA.DAS.EmployerFinance.Messages.Commands;

public class ImportAccountPaymentsCommand : Message
{
    public long AccountId { get; set; }
    public string PeriodEndRef { get; set; }
}