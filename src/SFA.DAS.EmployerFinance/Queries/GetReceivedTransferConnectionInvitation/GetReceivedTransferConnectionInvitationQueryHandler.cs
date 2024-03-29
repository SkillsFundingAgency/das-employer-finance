﻿using AutoMapper;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Queries.GetReceivedTransferConnectionInvitation;

public class GetReceivedTransferConnectionInvitationQueryHandler : IRequestHandler<GetReceivedTransferConnectionInvitationQuery, GetReceivedTransferConnectionInvitationResponse>
{
    private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
    private readonly IMapper _mapper;

    public GetReceivedTransferConnectionInvitationQueryHandler(ITransferConnectionInvitationRepository transferConnectionInvitationRepository, IMapper mapper)
    {
        _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
        _mapper = mapper;
    }

    public async Task<GetReceivedTransferConnectionInvitationResponse> Handle(GetReceivedTransferConnectionInvitationQuery message,CancellationToken cancellationToken)
    {
        var transferConnectionInvitation = await _transferConnectionInvitationRepository.GetByReceiver(
            Convert.ToInt32(message.TransferConnectionInvitationId.Value),
            message.AccountId,
            TransferConnectionInvitationStatus.Pending);

        return new GetReceivedTransferConnectionInvitationResponse
        {
            TransferConnectionInvitation = _mapper.Map<TransferConnectionInvitationDto>(transferConnectionInvitation)
        };
    }
}