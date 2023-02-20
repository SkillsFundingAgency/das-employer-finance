using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnections;

public class GetTransferConnectionsResponse
{
    public IEnumerable<TransferConnection> TransferConnections { get; set; }
}