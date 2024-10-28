using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Formatters.TransactionDownloads.Excel;

public class NonLevyExcelTransactionFormatter(IExcelService excelService) : ExcelTransactionFormatter(excelService), ITransactionFormatter
{
    public ApprenticeshipEmployerType ApprenticeshipEmployerType => ApprenticeshipEmployerType.NonLevy;

    protected override IEnumerable<string[]> GetTransactionRows(IEnumerable<TransactionDownloadLine> transactions)
    {
        return transactions.Select(transaction => new[]
        {
            transaction.DateCreated.ToString("dd/MM/yyyy"),
            transaction.TransactionType,
            transaction.Description,
            transaction.TrainingProvider,
            transaction.Uln,
            transaction.Apprentice,
            transaction.ApprenticeTrainingCourse,
            transaction.ApprenticeTrainingCourseLevel,
            transaction.PaidFromLevyFormatted,
            transaction.EmployerContributionFormatted,
            transaction.GovermentContributionFormatted,
            transaction.TotalFormatted
        });
    }

    protected override string[] GetHeaderRow()
    {
        return
        [
            "Transaction date", "Transaction type", "Description", "Training provider", "Unique learner number",
            "Apprentice", "Apprenticeship training course", "Course level", "Paid from transfer", "Your contribution",
            "Government contribution", "Total"
        ];
    }
}