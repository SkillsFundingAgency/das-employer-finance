using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.Messaging.Attributes;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Commands.CreateAccount
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand,Unit>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILog _logger;

        public CreateAccountCommandHandler(IAccountRepository accountRepository, ILog logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _accountRepository.CreateAccount(request.Id, request.Name);

                _logger.Info($"Account {request.Id} created");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not create account");
                throw;
            }
            return Unit.Value;
        }        
    }
}
