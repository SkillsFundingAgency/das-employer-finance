﻿using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Queries.GetRejectedTransferConnectionInvitation
{
    public class GetRejectedTransferConnectionInvitationQueryHandler 
        : IRequestHandler<GetRejectedTransferConnectionInvitationQuery, GetRejectedTransferConnectionInvitationResponse>
    {
        private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
        private readonly IMapper _mapper;

        public GetRejectedTransferConnectionInvitationQueryHandler(ITransferConnectionInvitationRepository transferConnectionInvitationRepository, IMapper mapper)
        {
            _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
            _mapper = mapper;
        }

        public async Task<GetRejectedTransferConnectionInvitationResponse> Handle(GetRejectedTransferConnectionInvitationQuery message,CancellationToken cancellationToken)
        {
            var transferConnectionInvitation = await _transferConnectionInvitationRepository.GetByReceiver(
                message.TransferConnectionInvitationId.Value,
                message.AccountId,
                TransferConnectionInvitationStatus.Rejected);

            return new GetRejectedTransferConnectionInvitationResponse
            {
                TransferConnectionInvitation = _mapper.Map<TransferConnectionInvitationDto>(transferConnectionInvitation)
            };
        }
    }
}