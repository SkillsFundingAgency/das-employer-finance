using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class TransactionViewModel
{
    public decimal CurrentBalance { get; set; }
    public AggregationData Data { get; set; }
}