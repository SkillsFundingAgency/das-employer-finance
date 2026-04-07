namespace SFA.DAS.EmployerFinance.Commands.PersistEnglishFractions;

public class PersistEnglishFractionsCommand : IRequest<PersistEnglishFractionsResponse>
{
    public string EmployerReference { get; set; }
    public bool UpdateRequired { get; set; }
    public DateTime DateCalculated { get; set; }
    public List<Models.Levy.DasEnglishFraction> Fractions { get; set; }
}
