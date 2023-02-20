
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;

public class GetLevyDeclarationResponse
{
    public List<LevyDeclarationItem> Declarations { get; set; }
}