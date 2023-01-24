using System;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Commands.LegalEntitySignAgreement
{
    public class LegalEntitySignAgreementCommand : IAuthorizationContextModel,IRequest<Unit>
    {
        public LegalEntitySignAgreementCommand(long signedAgreementId, int signedAgreementVersion, long accountId,
            long legalEntityId)
        {
            SignedAgreementId = signedAgreementId;
            SignedAgreementVersion = signedAgreementVersion;
            AccountId = accountId;
            LegalEntityId = legalEntityId;
        }


        public long SignedAgreementId { get; set; }
        public int SignedAgreementVersion { get; set; }
        public long AccountId { get; set; }
        public long LegalEntityId { get; set; }
    }

    public class LegalEntitySignAgreementCommandHandler : IRequestExceptionHandler<LegalEntitySignAgreementCommand,Unit>
    {
        private readonly IAccountLegalEntityRepository _accountLegalEntityRepository;
        private readonly ILog _logger;

        public LegalEntitySignAgreementCommandHandler(IAccountLegalEntityRepository accountLegalEntityRepository, ILog logger)
        {
            _accountLegalEntityRepository = accountLegalEntityRepository;
            _logger = logger;
        }

        public async Task Handle(LegalEntitySignAgreementCommand message)
        {
            try
            {
                await _accountLegalEntityRepository.SignAgreement(message.SignedAgreementId, message.SignedAgreementVersion,
                    message.AccountId, message.LegalEntityId);
                _logger.Info($"Signed agreement on legal entity {message.LegalEntityId}");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Could not sign agreement on legal entity");
                throw;
            }

            return Unit.Value;
        }
    }
}