using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.CreateAccountLegalEntity;

public class CreateAccountLegalEntityCommandHandler : IRequestHandler<CreateAccountLegalEntityCommand>
{
    private readonly IAccountLegalEntityRepository _accountLegalEntityRepository;
    private readonly ILogger<CreateAccountLegalEntityCommandHandler> _logger;

    public CreateAccountLegalEntityCommandHandler(IAccountLegalEntityRepository accountLegalEntityRepository, ILogger<CreateAccountLegalEntityCommandHandler> logger)
    {
        _accountLegalEntityRepository = accountLegalEntityRepository;
        _logger = logger;
    }

    public async Task Handle(CreateAccountLegalEntityCommand request,CancellationToken cancellationToken)
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
            _logger.LogInformation("Account Legal Entity {Id} created", request.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not create Account Legal Entity");
            throw;
        }
    }
}