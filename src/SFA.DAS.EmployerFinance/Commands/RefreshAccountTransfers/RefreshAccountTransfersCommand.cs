
namespace SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;

public class RefreshAccountTransfersCommand : IRequest<Unit>
{
    public long ReceiverAccountId { get; set; }
    public string PeriodEnd { get; set; }
    public Guid CorrelationId { get; set; }
}