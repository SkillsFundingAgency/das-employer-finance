using System.ComponentModel.DataAnnotations;
using System.Text;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Events;
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

        if (request.EmployerLevyData == null || request.EmployerLevyData.Count == 0)
        {
            return;
        }

        var levyTotalTransactionValue = decimal.Zero;

        foreach (var employerLevyData in request.EmployerLevyData)
        {
            var declarations = await levyImportCleanerStrategy.Cleanup(employerLevyData.EmpRef, employerLevyData.Declarations.Declarations);

            var levyImported = declarations.Length > 0;
            var levyTransactionValue = decimal.Zero;

            if (levyImported)
            {
                await dasLevyRepository.CreateEmployerDeclarations(declarations, employerLevyData.EmpRef, request.AccountId);

                logger.LogInformation("Processing declarations for {AccountId}, PAYE: {PayeRef}", request.AccountId, ObscurePayeScheme(employerLevyData.EmpRef));

                levyTransactionValue = await dasLevyRepository.ProcessDeclarations(request.AccountId, employerLevyData.EmpRef);
                levyTotalTransactionValue += levyTransactionValue;

                logger.LogInformation("Levy declarations processed for {AccountId}, PAYE: {PayeRef}, LevyTotal: {LevyTotal}", request.AccountId, ObscurePayeScheme(employerLevyData.EmpRef), levyTransactionValue);
            }

            var lastPositiveDeclaration = await dasLevyRepository.GetLastPositiveNetDeclarationForScheme(employerLevyData.EmpRef);
            var lastLevyDeclarationDate = lastPositiveDeclaration?.SubmissionDate;

            await PublishRefreshEmployerLevyDataCompletedEvent(
                levyImported,
                levyTransactionValue,
                request.AccountId,
                employerLevyData.EmpRef,
                lastLevyDeclarationDate);
        }

        await PublishAccountLevyStatusEvent(levyTotalTransactionValue, request.AccountId);
    }

    private async Task PublishRefreshEmployerLevyDataCompletedEvent(
        bool levyImported,
        decimal levyTransactionValue,
        long accountId,
        string payeRef,
        DateTime? lastLevyDeclarationDate)
    {
        logger.LogInformation(
            "Publishing RefreshEmployerLevyDataCompletedEvent levyImported {LevyImported}, levyTransactionValue {LevyTransactionValue}, payeRef {PayeRef}, lastLevyDeclarationDate {LastLevyDeclarationDate} for account {AccountId}",
            levyImported,
            levyTransactionValue,
            ObscurePayeScheme(payeRef),
            lastLevyDeclarationDate,
            accountId);

        await eventPublisher.Publish(new RefreshEmployerLevyDataCompletedEvent
        {
            AccountId = accountId,
            PayeRef = payeRef,
            LastLevyDeclarationDate = lastLevyDeclarationDate,
            Created = DateTime.UtcNow,
            LevyImported = levyImported,
            LevyTransactionValue = levyTransactionValue
        });

        logger.LogInformation("Published RefreshEmployerLevyDataCompletedEvent for account {AccountId}, PAYE {PayeRef}", accountId, ObscurePayeScheme(payeRef));
    }

    private async Task PublishAccountLevyStatusEvent(decimal levyTotalTransactionValue, long accountId)
    {
        if (levyTotalTransactionValue != decimal.Zero)
        {
            logger.LogInformation("Publishing LevyAddedToAccountEvent levyTotalTransactionValue {LevyTotalTransactionValue} for account {AccountId}", levyTotalTransactionValue, accountId);

            await eventPublisher.Publish(new LevyAddedToAccountEvent
            {
                AccountId = accountId,
                Amount = levyTotalTransactionValue
            });

            logger.LogInformation("Published LevyAddedToAccountEvent levyTotalTransactionValue {LevyTotalTransactionValue} for account {AccountId}", levyTotalTransactionValue, accountId);
        }
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
