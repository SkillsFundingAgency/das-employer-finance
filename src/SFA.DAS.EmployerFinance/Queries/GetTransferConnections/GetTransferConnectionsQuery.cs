using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnections;

public class GetTransferConnectionsQuery : IRequest<GetTransferConnectionsResponse>
{
    public long AccountId { get; set; }
    public TransferConnectionInvitationStatus Status { get; set; } = TransferConnectionInvitationStatus.Approved;
}