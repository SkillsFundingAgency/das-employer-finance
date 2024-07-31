using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.RenameAccount;

public class RenameAccountCommandHandler : IRequestHandler<RenameAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<RenameAccountCommandHandler> _logger;

    public RenameAccountCommandHandler(IAccountRepository accountRepository, ILogger<RenameAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task Handle(RenameAccountCommand request,CancellationToken cancellationToken)
    {
        try
        {
            await _accountRepository.RenameAccount(request.Id, request.Name);

            _logger.LogInformation($"Account {request.Id} renamed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not rename account");
            throw;
        }
    }
}