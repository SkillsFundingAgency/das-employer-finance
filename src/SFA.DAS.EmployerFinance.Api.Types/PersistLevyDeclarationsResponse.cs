namespace SFA.DAS.EmployerFinance.Api.Types;

public class PersistLevyDeclarationsResponse
{
    public int DeclarationsPersisted { get; set; }
    public int DeclarationsSkipped { get; set; }
    public int TransactionsCreated { get; set; }
}
