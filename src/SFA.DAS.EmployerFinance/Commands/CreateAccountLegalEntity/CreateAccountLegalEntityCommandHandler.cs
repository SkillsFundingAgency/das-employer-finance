﻿using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.CreateAccountLegalEntity;

public class CreateAccountLegalEntityCommandHandler : IRequestHandler<CreateAccountLegalEntityCommand,Unit>
{
    private readonly IAccountLegalEntityRepository _accountLegalEntityRepository;
    private readonly ILogger<CreateAccountLegalEntityCommandHandler> _logger;

    public CreateAccountLegalEntityCommandHandler(IAccountLegalEntityRepository accountLegalEntityRepository, ILogger<CreateAccountLegalEntityCommandHandler> logger)
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
            _logger.LogInformation($"Account Legal Entity {request.Id} created");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not create Account Legal Entity");
            throw;
        }
        return Unit.Value;
    }
}