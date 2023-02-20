using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.RejectTransferConnectionInvitation;

public class RejectTransferConnectionInvitationCommandHandler : IRequestHandler<RejectTransferConnectionInvitationCommand, Unit>
{
    private readonly IEmployerAccountRepository _employerAccountRepository;
    private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
    private readonly IUserAccountRepository _userRepository;

    public RejectTransferConnectionInvitationCommandHandler(
        IEmployerAccountRepository employerAccountRepository,
        ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
        IUserAccountRepository userRepository)
    {
        _employerAccountRepository = employerAccountRepository;
        _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(RejectTransferConnectionInvitationCommand request,CancellationToken cancellationToken)
    {
        var rejectorAccount = await _employerAccountRepository.Get(request.AccountId);
        var rejectorUser = await _userRepository.Get(request.UserRef);
        var transferConnectionInvitation = await _transferConnectionInvitationRepository.Get(request.TransferConnectionInvitationId.Value);

        transferConnectionInvitation.Reject(rejectorAccount, rejectorUser);

        return Unit.Value;
    }
}