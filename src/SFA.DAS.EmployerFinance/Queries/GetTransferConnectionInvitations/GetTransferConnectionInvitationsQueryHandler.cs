using AutoMapper;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;

public class GetTransferConnectionInvitationsQueryHandler : IRequestHandler<GetTransferConnectionInvitationsQuery, GetTransferConnectionInvitationsResponse>
{
    private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
    private readonly IMapper _mapper;

    public GetTransferConnectionInvitationsQueryHandler(ITransferConnectionInvitationRepository transferConnectionInvitationRepository, IMapper mapper)
    {
        _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
        _mapper = mapper;
    }

    public async Task<GetTransferConnectionInvitationsResponse> Handle(GetTransferConnectionInvitationsQuery message,CancellationToken  cancellationToken)
    {
        var transferConnectionInvitations = await _transferConnectionInvitationRepository.GetBySenderOrReceiver(message.AccountId);

        return new GetTransferConnectionInvitationsResponse
        {
            TransferConnectionInvitations = _mapper.Map<List<TransferConnectionInvitationDto>>(transferConnectionInvitations),
            AccountId = message.AccountId
        };
    }
}