namespace SFA.DAS.EmployerFinance.Queries.GetAccount;

public class GetAccountByIdRequest : IRequest<GetAccountByIdResponse>
{
    public long AccountId { get; set; }
}
