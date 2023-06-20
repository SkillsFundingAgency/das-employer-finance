using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;

public class GetEnglishFractionHistoryResposne
{
    public IEnumerable<DasEnglishFraction> FractionDetail { get; set; }
}