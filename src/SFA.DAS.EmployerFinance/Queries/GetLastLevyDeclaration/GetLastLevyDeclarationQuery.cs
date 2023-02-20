namespace SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;

public class GetLastLevyDeclarationQuery : IRequest<GetLastLevyDeclarationResponse>
{
    public string EmpRef { get; set; }
}