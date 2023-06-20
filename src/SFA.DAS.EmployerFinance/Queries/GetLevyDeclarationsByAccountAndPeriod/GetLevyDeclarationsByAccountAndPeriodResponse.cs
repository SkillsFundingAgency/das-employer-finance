using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndPeriod;

public class GetLevyDeclarationsByAccountAndPeriodResponse
{
    public List<LevyDeclarationItem> Declarations { get; set; }
}