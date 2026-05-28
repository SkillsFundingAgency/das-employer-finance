namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;

public class GetLevyDeclarationSubmissionIdsQuery : IRequest<List<long>>
{
    public string EmpRef { get; set; }
}
