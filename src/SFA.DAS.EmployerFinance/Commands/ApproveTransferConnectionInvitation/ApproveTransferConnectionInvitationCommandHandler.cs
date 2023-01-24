using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;

namespace SFA.DAS.EmployerFinance.Commands.ApproveTransferConnectionInvitation
{
    public class ApproveTransferConnectionInvitationCommandHandler : IRequestHandler<ApproveTransferConnectionInvitationCommand, Unit>
    {
        private readonly IEmployerAccountRepository _employerAccountRepository;
        private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
        private readonly IUserAccountRepository _userRepository;

        public ApproveTransferConnectionInvitationCommandHandler(
            IEmployerAccountRepository employerAccountRepository,
            ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
            IUserAccountRepository userRepository)
        {
            _employerAccountRepository = employerAccountRepository;
            _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(ApproveTransferConnectionInvitationCommand request, CancellationToken cancellationToken)
        {
            var approverAccount = await _employerAccountRepository.Get(request.AccountId);
            var approverUser = await _userRepository.Get(request.UserRef);
            var transferConnectionInvitation = await _transferConnectionInvitationRepository.Get(request.TransferConnectionInvitationId.Value);

            transferConnectionInvitation.Approve(approverAccount, approverUser);
            return Unit.Value;
        }
    }
}