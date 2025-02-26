﻿using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.Paye;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers.Payment;

public class CreateAccountPayeCommandHandler(
    IPayeRepository payeRepository,
    ILogger<CreateAccountPayeCommandHandler> logger)
    : IHandleMessages<CreateAccountPayeCommand>
{
    public async Task Handle(CreateAccountPayeCommand message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("Account Paye scheme created via {Mechanism} - Account Id: {AccountId}; Emp Ref: {EmpRef};",
                string.IsNullOrEmpty(message.Aorn) ? "Gov gateway" : "Aorn",
                message.AccountId,
                message.EmpRef);

            var payeScheme = new Paye(message.EmpRef, message.AccountId, message.Name, message.Aorn);
            await payeRepository.CreatePayeScheme(payeScheme);

            await GetLevyForNoneAornPayeSchemes(payeScheme, context);

            logger.LogInformation("Account Paye scheme created - Account Id: {AccountId}; Emp Ref: {EmpRef}", payeScheme.AccountId, payeScheme.EmpRef);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not create account paye scheme");
            throw;
        }
    }

    private async Task GetLevyForNoneAornPayeSchemes(Paye payeScheme, IMessageHandlerContext context)
    {
        if (string.IsNullOrEmpty(payeScheme.Aorn))
        {
            await context.SendLocal(new ImportAccountLevyDeclarationsCommand(payeScheme.AccountId, payeScheme.EmpRef));

            logger.LogInformation("Requested levy for - Account Id: {AccountId}; Emp Ref: {EmpRef}", payeScheme.AccountId, payeScheme.EmpRef);
        }
    }
}