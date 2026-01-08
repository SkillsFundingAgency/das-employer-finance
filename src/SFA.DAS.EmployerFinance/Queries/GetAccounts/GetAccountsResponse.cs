

using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetAccounts;

public class GetAccountsResponse
{
    public List<Account> Accounts { get; set; }

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
