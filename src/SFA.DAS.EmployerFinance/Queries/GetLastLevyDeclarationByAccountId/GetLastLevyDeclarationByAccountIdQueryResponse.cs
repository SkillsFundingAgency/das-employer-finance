using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclarationByAccountId;

public class GetLastLevyDeclarationByAccountIdQueryResponse
{
    public DasDeclaration Transaction { get; init; }
}