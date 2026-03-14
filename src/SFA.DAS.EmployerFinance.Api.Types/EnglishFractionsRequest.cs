namespace SFA.DAS.EmployerFinance.Api.Types;

public class EnglishFractionsRequest
{
    public string EmpRef { get; set; }
    public bool UpdateRequired { get; set; }
    public DateTime DateCalculated { get; set; }
    public List<EnglishFractionItem> Fractions { get; set; }
}
