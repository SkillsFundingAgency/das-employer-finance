using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Commands.RenameAccount
{
    public class RenameAccountCommandHandler : IRequestHandler<RenameAccountCommand,Unit>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILog _logger;

        public RenameAccountCommandHandler(IAccountRepository accountRepository, ILog logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(RenameAccountCommand request,CancellationToken cancellationToken)
        {
            try
            {
                await _accountRepository.RenameAccount(request.Id, request.Name);

                _logger.Info($"Account {request.Id} renamed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not rename account");
                throw;
            }

            return Unit.Value;
        }
    }
}
