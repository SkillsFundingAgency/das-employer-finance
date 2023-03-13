namespace SFA.DAS.EmployerFinance.Queries.GetTransferRequests;

public class GetTransferRequestsQuery : IRequest<GetTransferRequestsResponse>
{
    public long AccountId { get; set; }
}