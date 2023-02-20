using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnections
{
    public class GetTransferConnectionsQueryHandler : IRequestHandler<GetTransferConnectionsQuery, GetTransferConnectionsResponse>
    {
        private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
        private readonly IMapper _mapper;

        public GetTransferConnectionsQueryHandler(ITransferConnectionInvitationRepository transferConnectionInvitationRepository, IMapper mapper)
        {
            _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
            _mapper = mapper;
        }

        public async Task<GetTransferConnectionsResponse> Handle(GetTransferConnectionsQuery message,CancellationToken cancellationToken)
        {
            var transferConnectionInvitations = 
                await _transferConnectionInvitationRepository.GetByReceiver(message.AccountId, TransferConnectionInvitationStatus.Approved);
                
            return new GetTransferConnectionsResponse
            {
                TransferConnections = _mapper.Map<List<TransferConnectionInvitation>, List<TransferConnection>>(transferConnectionInvitations)
            };
        }
    }
}