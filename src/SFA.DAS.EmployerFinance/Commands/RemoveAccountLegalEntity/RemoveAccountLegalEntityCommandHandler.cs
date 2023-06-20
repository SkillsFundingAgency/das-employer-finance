using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.RemoveAccountLegalEntity;

public class RemoveAccountLegalEntityCommandHandler : IRequestHandler<RemoveAccountLegalEntityCommand,Unit>
{
    private readonly IAccountLegalEntityRepository _accountLegalEntityRepository;
    private readonly ILogger<RemoveAccountLegalEntityCommandHandler> _logger;

    public RemoveAccountLegalEntityCommandHandler(IAccountLegalEntityRepository accountLegalEntityRepository, ILogger<RemoveAccountLegalEntityCommandHandler> logger)
    {
        _accountLegalEntityRepository = accountLegalEntityRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveAccountLegalEntityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _accountLegalEntityRepository.RemoveAccountLegalEntity(request.Id);
            _logger.LogInformation($"Account Legal Entity {request.Id} removed");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not remove Account Legal Entity");
            throw;
        }

        return Unit.Value;
    }
}