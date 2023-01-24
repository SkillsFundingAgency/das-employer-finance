using MediatR;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd
{
    public class CreateNewPeriodEndCommand : IAuthorizationContextModel,IRequest<Unit>
    {
        public PeriodEnd NewPeriodEnd { get; set; }
    }
}
