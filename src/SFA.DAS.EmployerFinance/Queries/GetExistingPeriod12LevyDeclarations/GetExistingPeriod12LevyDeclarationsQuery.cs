using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;

public class GetExistingPeriod12LevyDeclarationsQuery : IRequest<List<ExistingPeriod12LevyDeclarationResult>>
{
    public string EmpRef { get; set; }
}
