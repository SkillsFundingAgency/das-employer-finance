namespace SFA.DAS.EmployerFinance.Models.Levy;

public class LevyPersistenceResult
{
    public int DeclarationsPersisted { get; set; }
    public decimal LevyTransactionValue { get; set; }
    public int TransactionsCreated { get; set; }
}
