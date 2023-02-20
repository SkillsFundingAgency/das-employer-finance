using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;

public class RefreshAccountTransfersCommand : IAuthorizationContextModel,IRequest<Unit>
{
    public long ReceiverAccountId { get; set; }
    public string PeriodEnd { get; set; }
    public Guid CorrelationId { get; set; }
}