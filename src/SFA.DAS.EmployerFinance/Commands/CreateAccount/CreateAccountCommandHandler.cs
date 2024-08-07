﻿using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.CreateAccount;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(IAccountRepository accountRepository, ILogger<CreateAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _accountRepository.CreateAccount(request.Id, request.Name);

            _logger.LogInformation("Account {Id} created", request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not create account");
            throw;
        }
    }        
}