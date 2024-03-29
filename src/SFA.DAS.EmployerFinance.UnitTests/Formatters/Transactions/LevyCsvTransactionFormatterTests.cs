﻿using System.Globalization;
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
            Assert.AreEqual(4, rows.Length);

            // We should have a header row in a specific format
            var headerColumns = rows[0].Split(char.Parse(","));
            
            Assert.AreEqual("Transaction date", headerColumns[0]);
            Assert.AreEqual("Transaction type", headerColumns[1]);
            Assert.AreEqual("Description", headerColumns[2]);
            Assert.AreEqual("PAYE scheme", headerColumns[3]);
            Assert.AreEqual("Payroll month", headerColumns[4]);
            Assert.AreEqual("Levy declared", headerColumns[5]);
            Assert.AreEqual("English %", headerColumns[6]);
            Assert.AreEqual("10% top up", headerColumns[7]);
            Assert.AreEqual("Training provider", headerColumns[8]);
            Assert.AreEqual("Unique learner number", headerColumns[9]);
            Assert.AreEqual("Apprentice", headerColumns[10]);
            Assert.AreEqual("Apprenticeship training course", headerColumns[11]);
            Assert.AreEqual("Course level", headerColumns[12]);
            Assert.AreEqual("Paid from levy", headerColumns[13]);
            Assert.AreEqual("Your contribution", headerColumns[14]);
            Assert.AreEqual("Government contribution", headerColumns[15]);
            Assert.AreEqual("Total", headerColumns[16]);
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
            Assert.AreEqual(4, rows.Length);
            
            var i = 1m;
            while (i < 4)
            {
                // We should be able to extract known values from each row
                var dataRow = rows[Convert.ToInt32(i)].Split(char.Parse(","));

                Assert.AreEqual(DateTime.Today.AddMonths(Convert.ToInt32(-i)).ToString("dd/MM/yyyy"), dataRow[0]);
                Assert.AreEqual($"{TransactionTypePrefix}{i}", dataRow[1]);
                Assert.AreEqual($"{DescriptionPrefix}{i}", dataRow[2]);
                Assert.AreEqual($"{EmpRefPrefix}{i}", dataRow[3]);
                Assert.AreEqual($"{PeriodEndPrefix}{i}", dataRow[4]);
                Assert.AreEqual((i * 1000).ToString("0.00000", CultureInfo.CurrentCulture), dataRow[5]); // LevyDeclared
                Assert.AreEqual((i * 10).ToString("0.00000", CultureInfo.CurrentCulture), dataRow[6]);
                Assert.AreEqual((i * 100).ToString("0.00000", CultureInfo.CurrentCulture), dataRow[7]);
                Assert.AreEqual($"\"{TrainingProviderPrefix}{i}\"", dataRow[8]);
                Assert.AreEqual($"{UlnPrefix}{i}", dataRow[9]);
                Assert.AreEqual($"{ApprenticePrefix}{i}", dataRow[10]);
                Assert.AreEqual($"{ApprenticeTrainingCoursePrefix}{i}", dataRow[11]);
                Assert.AreEqual($"{ApprenticeTrainingCourseLevel}{i}", dataRow[12]);
                Assert.AreEqual((i * 10).ToString("0.00000", CultureInfo.CurrentCulture), dataRow[13]);
                Assert.AreEqual((i).ToString("0.00000", CultureInfo.CurrentCulture), dataRow[14]);
                Assert.AreEqual((i * 10000).ToString("0.00000", CultureInfo.CurrentCulture), dataRow[15]);
                Assert.AreEqual(((i* 1000) + (i * 100)).ToString("0.00000", CultureInfo.CurrentCulture), dataRow[16]);
                i++;
            }
        }
    }
}
