using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.LegalEntitySignAgreement;

public class LegalEntitySignAgreementCommand : IRequest
{
    public LegalEntitySignAgreementCommand(long signedAgreementId, int signedAgreementVersion, long accountId, long legalEntityId)
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

public class LegalEntitySignAgreementCommandHandler : IRequestHandler<LegalEntitySignAgreementCommand>
{
    private readonly IAccountLegalEntityRepository _accountLegalEntityRepository;
    private readonly ILogger<LegalEntitySignAgreementCommandHandler> _logger;

    public LegalEntitySignAgreementCommandHandler(IAccountLegalEntityRepository accountLegalEntityRepository, ILogger<LegalEntitySignAgreementCommandHandler> logger)
    {
        _accountLegalEntityRepository = accountLegalEntityRepository;
        _logger = logger;
    }

    public async Task Handle(LegalEntitySignAgreementCommand message, CancellationToken cancellationToken)
    {
        try
        {
            await _accountLegalEntityRepository.SignAgreement(message.SignedAgreementId, message.SignedAgreementVersion,
                message.AccountId, message.LegalEntityId);
            _logger.LogInformation("Signed agreement on legal entity {LegalEntityId}.", message.LegalEntityId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not sign agreement on legal entity.");
            throw;
        }
    }
}