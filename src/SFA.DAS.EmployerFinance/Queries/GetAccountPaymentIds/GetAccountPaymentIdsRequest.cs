namespace SFA.DAS.EmployerFinance.Queries.GetAccountPaymentIds;

public class GetAccountPaymentIdsRequest : IRequest<GetAccountPaymentIdsResponse>
{
    public long AccountId { get; set; }
    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
