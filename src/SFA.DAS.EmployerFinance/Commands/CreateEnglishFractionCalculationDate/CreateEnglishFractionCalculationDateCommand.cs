
namespace SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;

public class CreateEnglishFractionCalculationDateCommand : IRequest<Unit>
{
    public DateTime DateCalculated { get; set; }
}