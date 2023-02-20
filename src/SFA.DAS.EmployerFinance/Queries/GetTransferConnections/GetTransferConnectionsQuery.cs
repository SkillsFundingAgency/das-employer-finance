namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnections;

public class GetTransferConnectionsQuery : IRequest<GetTransferConnectionsResponse>
{
    public long AccountId { get; set; }
}