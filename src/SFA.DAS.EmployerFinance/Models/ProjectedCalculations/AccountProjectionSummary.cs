namespace SFA.DAS.EmployerFinance.Models.ProjectedCalculations;

public class AccountProjectionSummary
{
    public long AccountId { get; set; }
    public DateTime ProjectionGenerationDate { get; set; }
    public ProjectedCalculation ProjectionCalulation { get; set; }
}