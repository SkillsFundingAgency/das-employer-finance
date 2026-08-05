using NUnit.Framework;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Types.UnitTests.Modles;

[Parallelizable]
public class WhenGetExpiredFundsByDate
{
    private ExpiredFunds _expiredFunds;
    private Dictionary<CalendarPeriod, decimal> _fundsIn;

    [SetUp]
    public void Arrange()
    {
        _expiredFunds = new ExpiredFunds();

        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), 9},
            {new CalendarPeriod(2018, 12), 8},
            {new CalendarPeriod(2019, 1), 5}
        };
    }

    [Test]
    public void Then_If_The_Date_Is_Not_Available_From_Expired_Funds_Zero_Is_Returned()
    {
        //Arrange
        var expiryPeriod = 2;
        var expiryDate = new DateTime(2020,01,01);

        //Act
        var actual = _expiredFunds.GetExpiringFundsByDate(_fundsIn, new Dictionary<CalendarPeriod, decimal>(), expiryDate,null, expiryPeriod);

        //Assert
        Assert.That(0m, Is.EqualTo(actual));
    }

    [Test]
    public void Then_The_Correct_Expiry_Amount_Is_Returned_From_The_Date_Provided()
    {
        //Arrange
        var expiryPeriod = 2;
        var expiryDate = new DateTime(2019, 1, 01);

        //Act
        var actual = _expiredFunds.GetExpiringFundsByDate(_fundsIn, new Dictionary<CalendarPeriod, decimal>(), expiryDate, null, expiryPeriod);

        //Assert
        Assert.That(9m, Is.EqualTo(actual));
    }
}