using NUnit.Framework;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Types.UnitTests.Models.CalendarPeriodTests;

[Parallelizable]
public class WhenSortingByCalendarPeriod
{
    [Test]
    public void Then_Calendar_Periods_Are_Sorted_Ascending_For_Months()
    {
        //Arrange
        var calendarList = new List<CalendarPeriod>
        {
            new CalendarPeriod(2018,05),
            new CalendarPeriod(2018,01),
            new CalendarPeriod(2018,03)
        };

        //Act
        var actual = calendarList.OrderBy(c => c).ToList();

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(3,Is.EqualTo(actual.Count));
        Assert.That(1, Is.EqualTo(actual[0].Month));
        Assert.That(3, Is.EqualTo(actual[1].Month));
        Assert.That(5, Is.EqualTo(actual[2].Month));
    }

    [Test]
    public void Then_Calendar_Periods_Are_Sorted_Ascending_For_Years()
    {
        //Arrange
        var calendarList = new List<CalendarPeriod>
        {
            new CalendarPeriod(2018,01),
            new CalendarPeriod(2019,01),
            new CalendarPeriod(2020,01)
        };

        //Act
        var actual = calendarList.OrderBy(c => c).ToList();

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(3, Is.EqualTo(actual.Count));
        Assert.That(2018, Is.EqualTo(actual[0].Year));
        Assert.That(2019, Is.EqualTo(actual[1].Year));
        Assert.That(2020, Is.EqualTo(actual[2].Year));
    }


    [Test]
    public void Then_Calendar_Periods_Are_Sorted_Ascending_For_Years_And_Months()
    {
        //Arrange
        var calendarList = new List<CalendarPeriod>
        {
            new CalendarPeriod(2020,03),
            new CalendarPeriod(2018,11),
            new CalendarPeriod(2018,9),
            new CalendarPeriod(2020,01)
        };

        //Act
        var actual = calendarList.OrderBy(c => c).ToList();

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(9, Is.EqualTo(actual[0].Month));
        Assert.That(2018, Is.EqualTo(actual[0].Year));
        Assert.That(11, Is.EqualTo(actual[1].Month));
        Assert.That(2018, Is.EqualTo(actual[1].Year));
        Assert.That(1, Is.EqualTo(actual[2].Month));
        Assert.That(2020, Is.EqualTo(actual[2].Year));
        Assert.That(3, Is.EqualTo(actual[3].Month));
        Assert.That(2020, Is.EqualTo(actual[3].Year));
    }
}