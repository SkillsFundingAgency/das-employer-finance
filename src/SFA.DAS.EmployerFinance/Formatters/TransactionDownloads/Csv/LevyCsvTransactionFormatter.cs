using System.Globalization;
using System.Text;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Formatters.TransactionDowloads.Csv;

public class LevyCsvTransactionFormatter : CsvTransactionFormatter, ITransactionFormatter
{
    public ApprenticeshipEmployerType ApprenticeshipEmployerType => ApprenticeshipEmployerType.Levy;

    protected override string CreateHeaders(bool isNewVersion)
    {
        var headerBuilder = new StringBuilder();
        if (isNewVersion)
        {
            CreateVersionTwoHeaders(headerBuilder);
        }
        else
        {
            CreateVersionOneHeaders(headerBuilder);    
        }
        

        return headerBuilder.ToString();
    }

    private static void CreateVersionOneHeaders(StringBuilder headerBuilder)
    {
        headerBuilder.Append("Transaction date,");
        headerBuilder.Append("Transaction type,");
        headerBuilder.Append("Description,");
        headerBuilder.Append("PAYE scheme,");
        headerBuilder.Append("Payroll month,");
        headerBuilder.Append("Levy declared,");
        headerBuilder.Append("English %,");
        headerBuilder.Append("10% top up,");
        headerBuilder.Append("Training provider,");
        headerBuilder.Append("Unique learner number,");
        headerBuilder.Append("Apprentice,");
        headerBuilder.Append("Apprenticeship training course,");
        headerBuilder.Append("Course level,");
        headerBuilder.Append("Paid from levy,");
        headerBuilder.Append("Your contribution,");
        headerBuilder.Append("Government contribution,");
        headerBuilder.Append("Total,");
    }
    
    private static void CreateVersionTwoHeaders(StringBuilder headerBuilder)
    {
        headerBuilder.Append("Transaction date,");
        headerBuilder.Append("Transaction type,");
        headerBuilder.Append("Description,");
        headerBuilder.Append("PAYE scheme,");
        headerBuilder.Append("Payroll month,");
        headerBuilder.Append("Levy declared,");
        headerBuilder.Append("English %,");
        headerBuilder.Append("10% top up,");
        headerBuilder.Append("Cohort reference,");
        headerBuilder.Append("Training provider,");
        headerBuilder.Append("Unique learner number,");
        headerBuilder.Append("Learner,");
        headerBuilder.Append("Course name,");
        headerBuilder.Append("Course level,");
        headerBuilder.Append("Course type,");
        headerBuilder.Append("Paid from levy,");
        headerBuilder.Append("Paid from transfer,");
        headerBuilder.Append("Your contribution,");
        headerBuilder.Append("Government contribution,");
        headerBuilder.Append("Total,");
    }

    protected override void WriteRowsCsv(IEnumerable<TransactionDownloadLine> transactions, StringBuilder builder, bool isNewVersion)
    {
        foreach (var transaction in transactions)
        {
            if (isNewVersion)
            {
                CreateVersionTwoRows(builder, transaction);
            }
            else
            {
                CreateVersionOneRows(builder, transaction);
            }
            builder.AppendLine();
        }

        // Get rid of last new line
        builder.Remove(builder.Length - 1, 1);
    }

    private static void CreateVersionOneRows(StringBuilder builder, TransactionDownloadLine transaction)
    {
        builder.Append($"{transaction.DateCreated:dd/MM/yyyy},");
        builder.Append($"{transaction.TransactionType},");
        builder.Append($"{transaction.Description?.Replace(",", "")},");
        builder.Append($"{transaction.PayeScheme},");
        builder.Append($"{transaction.PeriodEnd},");
        builder.Append($"{transaction.LevyDeclaredFormatted},");
        builder.Append($"{transaction.EnglishFractionFormatted},");
        builder.Append($"{transaction.TenPercentTopUpFormatted},");
        builder.Append($"{transaction.TrainingProviderFormatted?.Replace(",", "")},");
        builder.Append($"{transaction.Uln},");
        builder.Append($"{transaction.Apprentice},");
        builder.Append($"{transaction.ApprenticeTrainingCourse?.Replace(",", "")},");
        builder.Append($"{transaction.ApprenticeTrainingCourseLevel},");
        builder.Append($"{transaction.PaidFromLevyFormatted},");
        builder.Append($"{transaction.EmployerContributionFormatted},");
        builder.Append($"{transaction.GovermentContributionFormatted},");
        builder.Append($"{transaction.TotalFormatted},");
    }
    
    private static void CreateVersionTwoRows(StringBuilder builder, TransactionDownloadLine transaction)
    {
        builder.Append($"{transaction.DateCreated:dd/MM/yyyy},");
        builder.Append($"{transaction.TransactionType},");
        builder.Append($"{transaction.Description?.Replace(",", "")},");
        builder.Append($"{transaction.PayeScheme},");
        builder.Append($"{transaction.PeriodEnd},");
        builder.Append($"{transaction.LevyDeclaredFormatted},");
        builder.Append($"{transaction.EnglishFractionFormatted},");
        builder.Append($"{transaction.TenPercentTopUpFormatted},");
        builder.Append($"{transaction.CohortReference?.Replace(",", "")},");
        builder.Append($"{transaction.TrainingProviderFormatted?.Replace(",", "")},");
        builder.Append($"{transaction.Uln},");
        builder.Append($"{transaction.Apprentice},");
        builder.Append($"{transaction.ApprenticeTrainingCourse?.Replace(",", "")},");
        builder.Append($"{transaction.ApprenticeTrainingCourseLevel},");
        builder.Append($"{transaction.ApprenticeLearningTypeFormatted},");
        
        if (transaction.TransactionType != nameof(TransactionItemType.Transfer))
        {
            builder.Append($"{transaction.PaidFromLevyFormatted},");
            builder.Append($"{0m.ToString("0.00000", NumberFormatInfo.InvariantInfo)},");
        }
        else
        {
            builder.Append($"{0m.ToString("0.00000", NumberFormatInfo.InvariantInfo)},");
            builder.Append($"{transaction.PaidFromLevyFormatted},");
        }
        
        builder.Append($"{transaction.EmployerContributionFormatted},");
        builder.Append($"{transaction.GovermentContributionFormatted},");
        builder.Append($"{transaction.TotalFormatted},");
    }
}