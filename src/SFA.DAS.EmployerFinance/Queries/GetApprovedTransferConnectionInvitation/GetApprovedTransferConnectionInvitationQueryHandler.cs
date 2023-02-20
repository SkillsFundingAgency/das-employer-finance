using AutoMapper;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Queries.GetApprovedTransferConnectionInvitation;

public class GetApprovedTransferConnectionInvitationQueryHandler : IRequestHandler<GetApprovedTransferConnectionInvitationQuery, GetApprovedTransferConnectionInvitationResponse>
{
    private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
    private readonly IMapper _mapper;

    public GetApprovedTransferConnectionInvitationQueryHandler(ITransferConnectionInvitationRepository transferConnectionInvitationRepository, IMapper mapper)
    {
        _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
        _mapper = mapper;
    }

    public async Task<GetApprovedTransferConnectionInvitationResponse> Handle(GetApprovedTransferConnectionInvitationQuery message,CancellationToken cancellationToken)
    {
        var transferConnectionInvitation = await _transferConnectionInvitationRepository.GetByReceiver(
            message.TransferConnectionInvitationId.Value,
            message.AccountId,
            TransferConnectionInvitationStatus.Approved);

        return new GetApprovedTransferConnectionInvitationResponse
        {
            TransferConnectionInvitation = _mapper.Map<TransferConnectionInvitationDto>(transferConnectionInvitation)
        };
    }
}