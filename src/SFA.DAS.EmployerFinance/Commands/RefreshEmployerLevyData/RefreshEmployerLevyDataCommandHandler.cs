using System.ComponentModel.DataAnnotations;
using System.Text;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;

public class RefreshEmployerLevyDataCommandHandler(
    IValidator<RefreshEmployerLevyDataCommand> validator,
    IDasLevyRepository dasLevyRepository,
    ILevyImportCleanerStrategy levyImportCleanerStrategy,
    IEventPublisher eventPublisher,
    ILogger<RefreshEmployerLevyDataCommandHandler> logger)
    : IRequestHandler<RefreshEmployerLevyDataCommand>
{
    public async Task Handle(RefreshEmployerLevyDataCommand request, CancellationToken cancellationToken)
    {
        var result = validator.Validate(request);

        if (!result.IsValid())
        {
            throw new ValidationException(result.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var savedDeclarations = new List<DasDeclaration>();
        var updatedEmpRefs = new List<string>();

        foreach (var employerLevyData in request.EmployerLevyData)
        {
            var declarations = await levyImportCleanerStrategy.Cleanup(employerLevyData.EmpRef, employerLevyData.Declarations.Declarations);

            if (declarations.Length == 0) continue;

            await dasLevyRepository.CreateEmployerDeclarations(declarations, employerLevyData.EmpRef, request.AccountId);

            updatedEmpRefs.Add(employerLevyData.EmpRef);
            savedDeclarations.AddRange(declarations);
        }

        var hasDecalarations = savedDeclarations.Any();
        var levyTotalTransactionValue = decimal.Zero;

        if (hasDecalarations)
        {
            levyTotalTransactionValue = await HasAccountHadLevyTransactions(request, updatedEmpRefs);
        }

        await PublishRefreshEmployerLevyDataCompletedEvent(hasDecalarations, levyTotalTransactionValue, request.AccountId);
        await PublishAccountLevyStatusEvent(levyTotalTransactionValue, request.AccountId);
    }

    private async Task PublishRefreshEmployerLevyDataCompletedEvent(bool levyImported, decimal levyTotalTransactionValue, long accountId)
    {
        logger.LogInformation("Publishing RefreshEmployerLevyDataCompletedEvent levyImported {0}, levyTotalTransactionValue {1} for account {2}", levyImported, levyTotalTransactionValue, accountId);

        eventPublisher.Publish(new RefreshEmployerLevyDataCompletedEvent
        {
            AccountId = accountId,
            Created = DateTime.UtcNow,
            LevyImported = levyImported,
            LevyTransactionValue = levyTotalTransactionValue
        });
        logger.LogInformation("Published RefreshEmployerLevyDataCompletedEvent for account {0}", accountId);
    }

    private async Task PublishAccountLevyStatusEvent(decimal levyTotalTransactionValue, long accountId)
    {
        if (levyTotalTransactionValue != decimal.Zero)
        {
            logger.LogInformation("Publishing LevyAddedToAccount levyTotalTransactionValue {0} for account {1}", levyTotalTransactionValue, accountId);

            // This has been moved into a static class to overcome the bug we are seeing. If we publish LevyAddedToAccount without it inheriting from
            // Event base class, the system does not actually publish the event, it appears to create an error in the UoW (but no error is logged,
            // nor transaction rolled back). 
            //await LevyAddedToAccountWithEventBaseClass.Publish(eventPublisher, levyTotalTransactionValue, accountId);

            await eventPublisher.Publish(new LevyAddedToAccountEvent
            {
                AccountId = accountId,
                Amount = levyTotalTransactionValue
            });
            logger.LogInformation("Published LevyAddedToAccount levyTotalTransactionValue {0} for account {1}", levyTotalTransactionValue, accountId);
        }
    }

    private async Task<decimal> HasAccountHadLevyTransactions(RefreshEmployerLevyDataCommand message, IEnumerable<string> updatedEmpRefs)
    {
        var levyTransactionTotalAmount = decimal.Zero;

        foreach (var empRef in updatedEmpRefs)
        {
            logger.LogInformation("Processing declarations for {AccountId}, PAYE: {PayeRef}", message.AccountId, ObscurePayeScheme(empRef));
            levyTransactionTotalAmount += await dasLevyRepository.ProcessDeclarations(message.AccountId, empRef);
        }
        logger.LogInformation("Total levy declarations for {AccountId}, LevyTotal: {total}", message.AccountId, levyTransactionTotalAmount);

        return levyTransactionTotalAmount;
    }
    
    private static string ObscurePayeScheme(string payeSchemeId)
    {
        var length = payeSchemeId.Length;

        var response = new StringBuilder(payeSchemeId);

        for (var index = 1; index < length - 1; index++)
            if (response[index].ToString() != "/")
            {
                response.Remove(index, 1);
                response.Insert(index, "*");
            }

        return response.ToString();
    }
}