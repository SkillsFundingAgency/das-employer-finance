using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnections;

public class GetTransferConnectionsQueryHandler(
    ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
    IMapper mapper)
    : IRequestHandler<GetTransferConnectionsQuery, GetTransferConnectionsResponse>
{
    public async Task<GetTransferConnectionsResponse> Handle(GetTransferConnectionsQuery message, CancellationToken cancellationToken)
    {
        var transferConnectionInvitations = await transferConnectionInvitationRepository.GetByReceiver(message.AccountId, message.Status);

        return new GetTransferConnectionsResponse
        {
            TransferConnections = mapper.Map<List<TransferConnectionInvitation>, List<TransferConnection>>(transferConnectionInvitations)
        };
    }
}