using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Commands.ExpireAccountFunds;

public class ExpireAccountFundsCommand : IRequest<ExpireFundsResponse>
{
    public long AccountId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}
