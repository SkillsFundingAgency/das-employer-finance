using System.ComponentModel.DataAnnotations;
using System.Globalization;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;

public class PersistLevyDeclarationsCommandHandler(
    IValidator<PersistLevyDeclarationsCommand> validator,
    IDasLevyRepository dasLevyRepository,
    ILevyImportCleanerStrategy levyImportCleanerStrategy,
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
            "Persist levy declarations started for AccountId {AccountId}, EmpRef {EmpRef}, declarations received {Received}",
            data.AccountId,
            data.EmpRef,
            received);

        try
        {
            var dasDeclarations = data.Declarations.Select(ToDasDeclaration).ToArray();
            var cleaned = await levyImportCleanerStrategy.Cleanup(data.EmpRef, dasDeclarations);

            if (cleaned.Length == 0)
            {
                logger.LogInformation(
                    "Persist levy declarations: none remained after cleanup for AccountId {AccountId}, EmpRef {EmpRef}",
                    data.AccountId,
                    data.EmpRef);

                return new PersistLevyDeclarationsResponse
                {
                    DeclarationsPersisted = 0,
                    DeclarationsSkipped = received,
                    TransactionsCreated = 0
                };
            }

            await dasLevyRepository.CreateEmployerDeclarations(cleaned, data.EmpRef, data.AccountId);

            var levyTransactionValue = await dasLevyRepository.ProcessDeclarations(data.AccountId, data.EmpRef);

            var transactionsCreated = levyTransactionValue != 0m ? cleaned.Length : 0;

            logger.LogInformation(
                "Persist levy declarations completed for AccountId {AccountId}, EmpRef {EmpRef}, persisted {Persisted}, skipped {Skipped}, levy transaction total {LevyValue}, transactions created {TransactionsCreated}",
                data.AccountId,
                data.EmpRef,
                cleaned.Length,
                received - cleaned.Length,
                levyTransactionValue,
                transactionsCreated);

            return new PersistLevyDeclarationsResponse
            {
                DeclarationsPersisted = cleaned.Length,
                DeclarationsSkipped = received - cleaned.Length,
                TransactionsCreated = transactionsCreated
            };
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Persist levy declarations failed for AccountId {AccountId}, EmpRef {EmpRef}, declarations received {Received}",
                data.AccountId,
                data.EmpRef,
                received);
            throw;
        }
    }

    private static DasDeclaration ToDasDeclaration(NormalizedLevyDeclaration n) =>
        new()
        {
            Id = n.Id.ToString(CultureInfo.InvariantCulture),
            SubmissionDate = n.SubmissionDate,
            LevyDueYtd = n.LevyDueYtd,
            LevyAllowanceForFullYear = n.LevyAllowanceForFullYear,
            PayrollYear = n.PayrollYear ?? string.Empty,
            PayrollMonth = n.PayrollMonth,
            NoPaymentForPeriod = n.NoPaymentForPeriod,
            DateCeased = n.DateCeased,
            InactiveFrom = n.InactiveFrom,
            InactiveTo = n.InactiveTo,
            SubmissionId = n.SubmissionId,
            EndOfYearAdjustment = false,
            EndOfYearAdjustmentAmount = 0
        };
}
