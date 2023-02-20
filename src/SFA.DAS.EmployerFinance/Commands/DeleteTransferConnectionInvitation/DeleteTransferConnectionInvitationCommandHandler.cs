using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.DeleteSentTransferConnectionInvitation
{
    public class DeleteTransferConnectionInvitationCommandHandler : IRequestHandler<DeleteTransferConnectionInvitationCommand,Unit>
    {
        private readonly IEmployerAccountRepository _employerAccountRepository;
        private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
        private readonly IUserAccountRepository _userRepository;

        public DeleteTransferConnectionInvitationCommandHandler(
            IEmployerAccountRepository employerAccountRepository,
            ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
            IUserAccountRepository userRepository)
        {
            _employerAccountRepository = employerAccountRepository;
            _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(DeleteTransferConnectionInvitationCommand request,CancellationToken cancellationToken)
        {
            var deleterAccount = await _employerAccountRepository.Get(request.AccountId);
            var deleterUser = await _userRepository.Get(request.UserRef);
            var transferConnectionInvitation = await _transferConnectionInvitationRepository.Get(request.TransferConnectionInvitationId.Value);

            transferConnectionInvitation.Delete(deleterAccount, deleterUser);

            return Unit.Value;
        }
    }
}