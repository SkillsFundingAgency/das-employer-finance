namespace SFA.DAS.EmployerFinance.Models.Levy;

public class LevyDeclarationAndTopUpView
{
    public long AccountId { get; set; }
    public string PayrollYear { get; set; }
    public int PayrollMonth { get; set; }
    public int LastSubmission { get; set; }
    public bool EndOfYearAdjustment { get; set; }
    public decimal TotalAmount { get; set; }
}
