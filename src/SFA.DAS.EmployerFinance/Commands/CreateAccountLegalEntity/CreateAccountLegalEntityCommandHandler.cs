using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Commands.CreateAccountLegalEntity
{
    public class CreateAccountLegalEntityCommandHandler : IRequestHandler<CreateAccountLegalEntityCommand,Unit>
    {
        private readonly IAccountLegalEntityRepository _accountLegalEntityRepository;
        private readonly ILog _logger;

        public CreateAccountLegalEntityCommandHandler(IAccountLegalEntityRepository accountLegalEntityRepository, ILog logger)
        {
            _accountLegalEntityRepository = accountLegalEntityRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreateAccountLegalEntityCommand request,CancellationToken cancellationToken)
        {
            try
            {
                await _accountLegalEntityRepository.CreateAccountLegalEntity(
                    request.Id,
                    request.PendingAgreementId,
                    request.SignedAgreementId,
                    request.SignedAgreementVersion,
                    request.AccountId,
                    request.LegalEntityId
                );
                _logger.Info($"Account Legal Entity {request.Id} created");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Could not create Account Legal Entity");
                throw;
            }
            return Unit.Value;
        }
    }
}
