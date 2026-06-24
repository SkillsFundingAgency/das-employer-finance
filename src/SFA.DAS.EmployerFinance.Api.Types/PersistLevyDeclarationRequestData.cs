namespace SFA.DAS.EmployerFinance.Api.Types;

public class PersistLevyDeclarationRequestData
{
    public long AccountId { get; set; }
    public string EmpRef { get; set; } = string.Empty;
    public List<NormalizedLevyDeclaration> Declarations { get; set; } = [];
}