namespace SFA.DAS.EmployerFinance.Queries.GetAccountPaymentIds;

public class GetAccountPaymentIdsResponse
{
    public List<Guid> PaymentIds { get; set; }

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
