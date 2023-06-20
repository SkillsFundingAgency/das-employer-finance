
namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;

public class RefreshPaymentDataCommand : IRequest<Unit>
{
    public long AccountId { get; set; }
    public string PeriodEnd { get; set; }
    public Guid CorrelationId { get; set; }
}