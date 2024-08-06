
namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;

public class RefreshPaymentDataCommand : IRequest<RefreshPaymentDataResponse>
{
    public long AccountId { get; set; }
    public string PeriodEnd { get; set; }
    public Guid CorrelationId { get; set; }
}