namespace SFA.DAS.EmployerFinance.Api.Types;

public class AuditJobSummary
{
    public string Id { get; set; }

    public string Description { get; set; }

    public DateTime DateStarted { get; set; }

    public int NumRecords { get; set; }

    public int Running { get; set; }

    public int Completed { get; set; }

    public int Failed { get; set; }

    public string Status { get; set; }
}
