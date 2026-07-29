namespace SFA.DAS.EmployerFinance.Models.Levy;

public class LevyDeclarationEntity
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public string EmpRef { get; set; }
    public decimal? LevyDueYtd { get; set; }
    public decimal? LevyAllowanceForYear { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public long SubmissionId { get; set; }
    public string PayrollYear { get; set; }
    public short? PayrollMonth { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool EndOfYearAdjustment { get; set; }
    public decimal? EndOfYearAdjustmentAmount { get; set; }
    public DateTime? DateCeased { get; set; }
    public DateTime? InactiveFrom { get; set; }
    public DateTime? InactiveTo { get; set; }
    public long? HmrcSubmissionId { get; set; }
    public bool NoPaymentForPeriod { get; set; }
}
