namespace SFA.DAS.EmployerFinance.Queries.GetAccounts;

public class GetAccountsRequest : IRequest<GetAccountsResponse>
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
