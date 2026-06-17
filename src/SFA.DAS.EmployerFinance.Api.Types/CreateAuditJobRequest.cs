namespace SFA.DAS.EmployerFinance.Api.Types;

public class CreateAuditJobRequest
{
    public string Id { get; set; }

    public string Description { get; set; }

    public DateTime DateStarted { get; set; }

    public int NumRecords { get; set; }
}
