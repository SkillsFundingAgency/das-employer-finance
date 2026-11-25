using System.IO;
using System.Text.RegularExpressions;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.TransferStagedToOperationalTests;

public class WhenCopyingStagedTransfersToOperational
{
    [Test]
    public void ThenCopiesTransferStagingRowsStraightIntoAccountTransfers()
    {
        var sql = File.ReadAllText(GetStoredProcedurePath());
        var transferInsert = ExtractTransferInsertSection(sql);

        transferInsert.Should().Contain("FROM employer_financial.TransferStaging ts");
        transferInsert.Should().Contain("ts.Amount");
        transferInsert.Should().NotContain("SUM(");
        transferInsert.Should().NotContain("GROUP BY");
        transferInsert.Should().NotContain("employer_financial.Payment");
    }

    private static string ExtractTransferInsertSection(string sql)
    {
        var match = Regex.Match(
            sql,
            @"-- 3\. Insert AccountTransfers from TransferStaging(?<section>.*?)-- 4\. Insert TransactionLines",
            RegexOptions.Singleline);

        match.Success.Should().BeTrue("expected transfer insert section markers in TransferStagedToOperational.sql");
        return match.Groups["section"].Value;
    }

    private static string GetStoredProcedurePath()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "SFA.DAS.EmployerFinance.Database",
                "StoredProcedures",
                "TransferStagedToOperational.sql");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find TransferStagedToOperational.sql from the test directory.");
    }
}
