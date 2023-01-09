using System;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Formatters;
using SFA.DAS.EmployerFinance.Time;

namespace SFA.DAS.EmployerFinance.UnitTests.Formatters
{
    [TestFixture]
    public class DateTimeStringFormatterTests
    {
        [TestCase(2021, 2, 1, "2020/21")]
        [TestCase(2022, 2, 1, "2021/22")]
        [TestCase(2023, 2, 1, "2022/23")]
        [TestCase(2022, 4, 1, "2021/22")]
        [TestCase(2022, 4, 20, "2022/23")]
        [TestCase(2022, 5, 1, "2022/23")]
        [TestCase(2023, 5, 1, "2023/24")]
        public void FinancialYearStringForReturnsCorrectString(int year, int month, int day, string expected)
        {
            var testDateTime = new DateTime(year, month, day);
            var referenceDate = new DateTime(year, month, day);
            var formatter = new DateTimeStringFormatter(new CurrentDateTime(referenceDate));
            var result = formatter.FinancialYearStringFor(testDateTime);
            Assert.AreEqual(expected, result);
        }

        [TestCase(2021, 4, 20, "2022/23")]
        [TestCase(2022, 4, 20, "2023/24")]
        [TestCase(2022, 5, 1, "2023/24")]
        [TestCase(2023, 5, 1, "2024/25")]
        public void ToNextFinancialYearStringForReturnsCorrectString(int year, int month, int day, string expected)
        {
            var testDateTime = new DateTime(year, month, day);
            var referenceDate = new DateTime(year, month, day);
            var formatter = new DateTimeStringFormatter(new CurrentDateTime(referenceDate));
            var result = formatter.FinancialYearStringFor(testDateTime.AddYears(1));
            Assert.AreEqual(expected, result);
        }

        [TestCase(2021, 4, 20, "2023/24")]
        [TestCase(2022, 4, 20, "2024/25")]
        [TestCase(2022, 5, 1, "2024/25")]
        [TestCase(2023, 5, 1, "2025/26")]
        public void ToYearAfterFinancialYearStringForReturnsCorrectString(int year, int month, int day, string expected)
        {
            var testDateTime = new DateTime(year, month, day);
            var referenceDate = new DateTime(year, month, day);
            var formatter = new DateTimeStringFormatter(new CurrentDateTime(referenceDate));
            var result = formatter.FinancialYearStringFor(testDateTime.AddYears(2));
            Assert.AreEqual(expected, result);
        }
    }
}
