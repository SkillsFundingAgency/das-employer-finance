namespace SFA.DAS.EmployerFinance.Api.Types;

public class NormalizedLevyDeclaration
{
    public DateTime SubmissionDate { get; set; }
    public long Id { get; set; }
    public short? PayrollMonth { get; set; }
    public string PayrollYear { get; set; } = string.Empty;
    public decimal LevyAllowanceForFullYear { get; set; }
    public decimal? LevyDueYtd { get; set; }
    public bool NoPaymentForPeriod { get; set; }
    public DateTime? DateCeased { get; set; }
    public DateTime? InactiveFrom { get; set; }
    public DateTime? InactiveTo { get; set; }
    public long SubmissionId { get; set; }
}