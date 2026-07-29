namespace SFA.DAS.EmployerFinance.Commands.TransferStagedToOperational;

public class TransferStagedToOperationalCommand : IRequest<TransferStagedToOperationalResponse>
{
    public long AccountId { get; set; }
    public string PeriodEnd { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
