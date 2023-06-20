using AutoMapper;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitation;

public class GetTransferConnectionInvitationQueryHandler : IRequestHandler<GetTransferConnectionInvitationQuery, GetTransferConnectionInvitationResponse>
{
    private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
    private readonly IMapper _mapper;

    public GetTransferConnectionInvitationQueryHandler(ITransferConnectionInvitationRepository transferConnectionInvitationRepository, IMapper mapper)
    {
        _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
        _mapper = mapper;
    }

    public async Task<GetTransferConnectionInvitationResponse> Handle(GetTransferConnectionInvitationQuery message,CancellationToken cancellationToken)
    {
        var transferConnectionInvitation = await _transferConnectionInvitationRepository.GetBySenderOrReceiver(
            Convert.ToInt32(message.TransferConnectionInvitationId.Value),
            message.AccountId);

        return new GetTransferConnectionInvitationResponse
        {
            TransferConnectionInvitation = _mapper.Map<TransferConnectionInvitationDto>(transferConnectionInvitation)
        };
    }
}