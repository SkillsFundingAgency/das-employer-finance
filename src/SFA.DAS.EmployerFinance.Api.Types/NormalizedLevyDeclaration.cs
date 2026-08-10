namespace SFA.DAS.EmployerFinance.Api.Types;

public class NormalizedLevyDeclaration
{
    public string Id { get; set; } = string.Empty;
    public decimal? LevyDueYtd { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string SubmissionType { get; set; } = string.Empty;
    public decimal LevyAllowanceForFullYear { get; set; }
    public string PayrollYear { get; set; } = string.Empty;
    public short? PayrollMonth { get; set; }
    public bool NoPaymentForPeriod { get; set; }
    public DateTime? DateCeased { get; set; }
    public DateTime? InactiveFrom { get; set; }
    public DateTime? InactiveTo { get; set; }
    public bool EndOfYearAdjustment { get; set; }
    public decimal EndOfYearAdjustmentAmount { get; set; }
    public long SubmissionId { get; set; }
}
