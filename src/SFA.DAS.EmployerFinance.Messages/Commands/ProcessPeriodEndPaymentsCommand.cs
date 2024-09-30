namespace SFA.DAS.EmployerFinance.Messages.Commands;

public class ProcessPeriodEndPaymentsCommand : Message
{
    public string PeriodEndRef { get; init; }
    public int BatchNumber { get; init; } = 0;
}