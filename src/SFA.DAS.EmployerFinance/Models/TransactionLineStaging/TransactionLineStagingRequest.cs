namespace SFA.DAS.EmployerFinance.Models.TransactionLineStaging;

public class TransactionLineStagingRequest
{
    public List<TransactionLineStagingModel> TransactionLines { get; set; } = new();
}