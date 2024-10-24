using SFA.DAS.EmployerFinance.Extensions;

namespace SFA.DAS.EmployerFinance.UnitTests.Extensions
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [TestCase(2022, 2, 1, "2021/22")]
        [TestCase(2022, 4, 1, "2021/22")]
        [TestCase(2022, 4, 20, "2022/23")]
        [TestCase(2022, 5, 1, "2022/23")]
        public void ToFinancialYearStringReturnsCorrectString(int year, int month, int day, string expected)
        {
            var dateTime = new DateTime(year, month, day);
            var result = dateTime.ToFinancialYearString();
            result.Should().Be(expected);
        }

        [TestCase(2022, 4, 20, "2023/24")]
        [TestCase(2022, 5, 1, "2023/24")]
        public void ToNextFinancialYearStringReturnsCorrectString(int year, int month, int day, string expected)
        {
            var dateTime = new DateTime(year, month, day);
            var result = dateTime.AddYears(1).ToFinancialYearString();
            result.Should().Be(expected);
        }

        [TestCase(2022, 4, 20, "2024/25")]
        [TestCase(2022, 5, 1, "2024/25")]
        public void ToYearAfterNextFinancialYearStringReturnsCorrectString(int year, int month, int day, string expected)
        {
            var dateTime = new DateTime(year, month, day);
            var result = dateTime.AddYears(2).ToFinancialYearString();
            result.Should().Be(expected);
        }

        [TestCase(2023, 12, 31, "23-24")]
        [TestCase(2024, 3, 31, "23-24")]
        [TestCase(2024, 4, 21, "24-25")]
        public void ToPayrollYearStart_ReturnsCorrectString(int year, int month, int day, string expected)
        {
            // Arrange
            var date = new DateTime(year, month, day);

            // Act
            var result = date.ToPayrollYearString();

            // Assert
            result.Should().Be(expected);
        }
    }
}
