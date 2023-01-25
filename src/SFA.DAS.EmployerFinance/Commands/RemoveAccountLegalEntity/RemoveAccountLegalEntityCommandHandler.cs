using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Commands.RemoveAccountLegalEntity
{
    public class RemoveAccountLegalEntityCommandHandler : IRequestHandler<RemoveAccountLegalEntityCommand,Unit>
    {
        private readonly IAccountLegalEntityRepository _accountLegalEntityRepository;
        private readonly ILog _logger;

        public RemoveAccountLegalEntityCommandHandler(IAccountLegalEntityRepository accountLegalEntityRepository, ILog logger)
        {
            _accountLegalEntityRepository = accountLegalEntityRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(RemoveAccountLegalEntityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _accountLegalEntityRepository.RemoveAccountLegalEntity(request.Id);
                _logger.Info($"Account Legal Entity {request.Id} removed");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Could not remove Account Legal Entity");
                throw;
            }

            return Unit.Value;
        }
    }
}
