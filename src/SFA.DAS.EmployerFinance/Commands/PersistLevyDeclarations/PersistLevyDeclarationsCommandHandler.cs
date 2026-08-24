using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;

public class PersistLevyDeclarationsCommandHandler(
    IValidator<PersistLevyDeclarationsCommand> validator,
    IDasLevyRepository dasLevyRepository,
    ILogger<PersistLevyDeclarationsCommandHandler> logger)
    : IRequestHandler<PersistLevyDeclarationsCommand, PersistLevyDeclarationsResponse>
{
    public async Task<PersistLevyDeclarationsResponse> Handle(PersistLevyDeclarationsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var data = request.Data;
        var received = data.Declarations.Count;

        logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Persist levy declarations started for AccountId {AccountId}, EmpRef {EmpRef}, declarations received {Received}, GenerateTransactions {GenerateTransactions}",
            data.CorrelationId,
            data.AccountId,
            data.EmpRef,
            received,
            data.GenerateTransactions);

        try
        {
            var declarations = data.Declarations.Select(ToDasDeclaration).ToArray();
            var persistenceResult = await dasLevyRepository.PersistLevyDeclarations(
                declarations,
                data.EmpRef,
                data.AccountId,
                data.GenerateTransactions,
                cancellationToken);
            var declarationsSkipped = received - persistenceResult.DeclarationsPersisted;

            logger.LogInformation(
                "[CorrelationId: {CorrelationId}] Persist levy declarations completed for AccountId {AccountId}, EmpRef {EmpRef}, persisted {Persisted}, skipped {Skipped}, levy transaction total {LevyValue}, transactions created {TransactionsCreated}",
                data.CorrelationId,
                data.AccountId,
                data.EmpRef,
                persistenceResult.DeclarationsPersisted,
                declarationsSkipped,
                persistenceResult.LevyTransactionValue,
                persistenceResult.TransactionsCreated);

            return new PersistLevyDeclarationsResponse
            {
                DeclarationsReceived = received,
                DeclarationsPersisted = persistenceResult.DeclarationsPersisted,
                DeclarationsSkipped = declarationsSkipped,
                LevyTransactionValue = persistenceResult.LevyTransactionValue,
                TransactionsCreated = persistenceResult.TransactionsCreated
            };
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[CorrelationId: {CorrelationId}] Persist levy declarations failed for AccountId {AccountId}, EmpRef {EmpRef}, declarations received {Received}",
                data.CorrelationId,
                data.AccountId,
                data.EmpRef,
                received);
            throw;
        }
    }

    private static DasDeclaration ToDasDeclaration(NormalizedLevyDeclaration n) =>
        new()
        {
            Id = n.Id,
            SubmissionDate = n.SubmissionDate,
            SubmissionType = n.SubmissionType,
            LevyDueYtd = n.LevyDueYtd,
            LevyAllowanceForFullYear = n.LevyAllowanceForFullYear,
            PayrollYear = n.PayrollYear ?? string.Empty,
            PayrollMonth = n.PayrollMonth,
            NoPaymentForPeriod = n.NoPaymentForPeriod,
            DateCeased = n.DateCeased,
            InactiveFrom = n.InactiveFrom,
            InactiveTo = n.InactiveTo,
            SubmissionId = n.SubmissionId,
            EndOfYearAdjustment = n.EndOfYearAdjustment,
            EndOfYearAdjustmentAmount = n.EndOfYearAdjustmentAmount
        };
}
