using System.Globalization;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads.Csv;

namespace SFA.DAS.EmployerFinance.UnitTests.Formatters.Transactions
{
    public class LevyCsvTransactionFormatterTests : BaseFormatterTest
    {
        public override ITransactionFormatter PaymentFormatter => new LevyCsvTransactionFormatter();

        public override string ExpectedMimeType => "text/csv";

        public override string ExpectedFileExtension => "csv";

        public override DownloadFormatType ExpectedDownloadFormats => DownloadFormatType.CSV;

        [Test]
        public void WhenICallGetContentsIGetCorrectHeaderFormat()
        {
            var formattedFileData = PaymentFormatter.GetFileData(TransactionDownloadLines);

            var formattedFileContent = System.Text.Encoding.UTF8.GetString(formattedFileData);

            var rows = formattedFileContent.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );

            // We should have 1 header and 3 data rows
            rows.Length.Should().Be(4);

            // We should have a header row in a specific format
            var headerColumns = rows[0].Split(char.Parse(","));
            
            headerColumns[0].Should().Be("Transaction date");
            headerColumns[1].Should().Be("Transaction type");
            headerColumns[2].Should().Be("Description");
            headerColumns[3].Should().Be("PAYE scheme");
            headerColumns[4].Should().Be("Payroll month");
            headerColumns[5].Should().Be("Levy declared");
            headerColumns[6].Should().Be("English %");
            headerColumns[7].Should().Be("10% top up");
            headerColumns[8].Should().Be("Training provider");
            headerColumns[9].Should().Be("Unique learner number");
            headerColumns[10].Should().Be("Apprentice");
            headerColumns[11].Should().Be("Apprenticeship training course");
            headerColumns[12].Should().Be("Course level");
            headerColumns[13].Should().Be("Paid from levy");
            headerColumns[14].Should().Be("Your contribution");
            headerColumns[15].Should().Be("Government contribution");
            headerColumns[16].Should().Be("Total");
        }

        [Test]
        public void WhenICallGetContentsIGetCorrectDataFormat()
        {
            var formattedFileData = PaymentFormatter.GetFileData(TransactionDownloadLines); 

            var formattedFileContent = System.Text.Encoding.UTF8.GetString(formattedFileData);

            var rows = formattedFileContent.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );

            // We should have 1 header and 3 data rows
            rows.Length.Should().Be(4);
            
            var i = 1m;
            while (i < 4)
            {
                // We should be able to extract known values from each row
                var dataRow = rows[Convert.ToInt32(i)].Split(char.Parse(","));

                dataRow[0].Should().Be(DateTime.Today.AddMonths(Convert.ToInt32(-i)).ToString("dd/MM/yyyy"));
                dataRow[1].Should().Be($"{TransactionTypePrefix}{i}");
                dataRow[2].Should().Be($"{DescriptionPrefix}{i}");
                dataRow[3].Should().Be($"{EmpRefPrefix}{i}");
                dataRow[4].Should().Be($"{PeriodEndPrefix}{i}");
                dataRow[5].Should().Be((i * 1000).ToString("0.00000", CultureInfo.CurrentCulture)); // LevyDeclared
                dataRow[6].Should().Be((i * 10).ToString("0.00000", CultureInfo.CurrentCulture));
                dataRow[7].Should().Be((i * 100).ToString("0.00000", CultureInfo.CurrentCulture));
                dataRow[8].Should().Be($"\"{TrainingProviderPrefix}{i}\"");
                dataRow[9].Should().Be($"{UlnPrefix}{i}");
                dataRow[10].Should().Be($"{ApprenticePrefix}{i}");
                dataRow[11].Should().Be($"{ApprenticeTrainingCoursePrefix}{i}");
                dataRow[12].Should().Be($"{ApprenticeTrainingCourseLevel}{i}");
                dataRow[13].Should().Be((i * 10).ToString("0.00000", CultureInfo.CurrentCulture));
                dataRow[14].Should().Be((i).ToString("0.00000", CultureInfo.CurrentCulture));
                dataRow[15].Should().Be((i * 10000).ToString("0.00000", CultureInfo.CurrentCulture));
                dataRow[16].Should().Be(((i * 1000) + (i * 100)).ToString("0.00000", CultureInfo.CurrentCulture));
                i++;
            }
        }
    }
}
