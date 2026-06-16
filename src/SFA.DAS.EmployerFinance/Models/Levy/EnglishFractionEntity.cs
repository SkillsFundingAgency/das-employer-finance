namespace SFA.DAS.EmployerFinance.Models.Levy;

public class EnglishFractionEntity
{
    public long Id { get; set; }
    public DateTime DateCalculated { get; set; }
    public decimal? Amount { get; set; }
    public string EmpRef { get; set; }
    public DateTime DateCreated { get; set; }
}
