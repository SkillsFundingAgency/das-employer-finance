using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionsUpdateRequired;

namespace SFA.DAS.EmployerFinance.Commands.UpdateEnglishFractions;

public class UpdateEnglishFractionsCommand : IRequest<Unit>
{
    public string EmployerReference { get; set; }
    public GetEnglishFractionUpdateRequiredResponse EnglishFractionUpdateResponse { get; set; }
}