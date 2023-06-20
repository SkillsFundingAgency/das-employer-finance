namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;

public class GetLevyDeclarationRequest : IRequest<GetLevyDeclarationResponse>
{
    public string HashedAccountId { get; set; }
}