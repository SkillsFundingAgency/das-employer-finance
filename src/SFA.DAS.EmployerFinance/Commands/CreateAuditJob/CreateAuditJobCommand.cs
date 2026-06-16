namespace SFA.DAS.EmployerFinance.Commands.CreateAuditJob;

public class CreateAuditJobCommand : IRequest<CreateAuditJobCommandResult>
{
    public string JobId { get; set; }

    public string Description { get; set; }

    public DateTime DateStarted { get; set; }

    public int NumRecords { get; set; }
}
