using MediatR;

namespace SFA.DAS.EmployerFinance.Queries.GetHMRCLevyDeclaration
{
    public class GetHMRCLevyDeclarationQuery : IRequest<GetHMRCLevyDeclarationResponse>
    {
        public string EmpRef { get; set; }
    }
}
