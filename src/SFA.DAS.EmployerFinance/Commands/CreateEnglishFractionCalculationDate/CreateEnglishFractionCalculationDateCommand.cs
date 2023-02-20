using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;

public class CreateEnglishFractionCalculationDateCommand : IAuthorizationContextModel,IRequest<Unit>
{
    public DateTime DateCalculated { get; set; }
}