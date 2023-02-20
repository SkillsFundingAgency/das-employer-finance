using AutoMapper;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.TransferConnections;

namespace SFA.DAS.EmployerFinance.Queries.GetSentTransferConnectionInvitation;

public class GetSentTransferConnectionInvitationQueryHandler : IRequestHandler<GetSentTransferConnectionInvitationQuery, GetSentTransferConnectionInvitationResponse>
{
    private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
    private readonly IMapper _mapper;

    public GetSentTransferConnectionInvitationQueryHandler(ITransferConnectionInvitationRepository transferConnectionInvitationRepository, IMapper mapper)
    {
        _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
        _mapper = mapper;
    }

    public async Task<GetSentTransferConnectionInvitationResponse> Handle(GetSentTransferConnectionInvitationQuery message,CancellationToken cancellationToken)
    {
        var transferConnectionInvitation = await _transferConnectionInvitationRepository.GetBySender(
            message.TransferConnectionInvitationId.Value,
            message.AccountId,
            TransferConnectionInvitationStatus.Pending);

        return new GetSentTransferConnectionInvitationResponse
        {
            TransferConnectionInvitation = _mapper.Map<TransferConnectionInvitationDto>(transferConnectionInvitation)
        };
    }
}