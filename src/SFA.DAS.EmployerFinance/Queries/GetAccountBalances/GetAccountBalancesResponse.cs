using SFA.DAS.EmployerFinance.Models.Account;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountBalances;

public class GetAccountBalancesResponse
{
    public List<AccountBalance> Accounts { get; set; }
}