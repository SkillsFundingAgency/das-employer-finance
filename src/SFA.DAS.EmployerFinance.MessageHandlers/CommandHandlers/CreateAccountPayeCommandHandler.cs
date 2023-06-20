using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.Paye;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;

public class CreateAccountPayeCommandHandler : IHandleMessages<CreateAccountPayeCommand>
{
    private readonly IPayeRepository _payeRepository;
    private readonly ILogger<CreateAccountPayeCommandHandler> _logger;

    public CreateAccountPayeCommandHandler(IPayeRepository payeRepository, ILogger<CreateAccountPayeCommandHandler> logger)
    {
        _payeRepository = payeRepository;
        _logger = logger;
    }

    public async Task Handle(CreateAccountPayeCommand message, IMessageHandlerContext context)
    {
        try
        {
            _logger.LogInformation($"Account Paye scheme created via {(string.IsNullOrEmpty(message.Aorn) ? "Gov gateway" : "Aorn")} - Account Id: {message.AccountId}; Emp Ref: {message.EmpRef};");

            var payeScheme = new Paye(message.EmpRef, message.AccountId, message.Name, message.Aorn);
            await _payeRepository.CreatePayeScheme(payeScheme);

            await GetLevyForNoneAornPayeSchemes(payeScheme, context);

            _logger.LogInformation($"Account Paye scheme created - Account Id: {payeScheme.AccountId}; Emp Ref: {payeScheme.EmpRef}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not create account paye scheme");
            throw;
        }
    }

    private async Task GetLevyForNoneAornPayeSchemes(Paye payeScheme, IMessageHandlerContext context)
    {
        if (string.IsNullOrEmpty(payeScheme.Aorn))
        {
            await context.SendLocal(new ImportAccountLevyDeclarationsCommand(payeScheme.AccountId, payeScheme.EmpRef));

            _logger.LogInformation($"Requested levy for - Account Id: {payeScheme.AccountId}; Emp Ref: {payeScheme.EmpRef}");
        }
    }
}