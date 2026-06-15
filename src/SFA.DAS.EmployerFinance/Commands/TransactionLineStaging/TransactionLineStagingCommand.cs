using SFA.DAS.EmployerFinance.Models.TransactionLineStaging;

namespace SFA.DAS.EmployerFinance.Commands.TransactionLineStaging;

public class TransactionLineStagingCommand : IRequest<TransactionLineStagingResponse>
{
    public string CorrelationId { get; set; }
    public List<TransactionLineStagingModel> TransactionLines { get; set; } = new();
}
