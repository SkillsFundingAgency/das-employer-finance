using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;

public class CreateNewPeriodEndCommand : IRequest<Unit>
{
    public PeriodEnd NewPeriodEnd { get; set; }
}