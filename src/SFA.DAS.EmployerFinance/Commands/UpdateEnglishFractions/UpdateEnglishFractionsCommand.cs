using MediatR;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionsUpdateRequired;

namespace SFA.DAS.EmployerFinance.Commands.UpdateEnglishFractions
{
    public class UpdateEnglishFractionsCommand : IAuthorizationContextModel,IRequest<Unit>
    {
        public string EmployerReference { get; set; }
        public GetEnglishFractionUpdateRequiredResponse EnglishFractionUpdateResponse { get; set; }
    }
}
