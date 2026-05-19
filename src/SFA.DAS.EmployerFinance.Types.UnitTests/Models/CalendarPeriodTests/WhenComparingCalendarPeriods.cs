using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.Testing;

namespace SFA.DAS.EmployerFinance.Types.UnitTests.Models.CalendarPeriodTests;

[Parallelizable]
public class WhenComparingCalendarPeriods : FluentTest<WhenComparingCalendarPeriodsFixture>
{
    [TestCase(2018, 01, true)]
    [TestCase(2018, 02, false)]
    [TestCase(2017, 12, false)]
    [TestCase(2019, 12, false)]
    public void Then_The_Equals_Operator_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1 == f.CalendarPeriod2, (f, r) => r.Should().Be(expected));
    }
    
    [TestCase(2018, 01, true)]
    [TestCase(2018, 02, false)]
    [TestCase(2017, 12, false)]
    [TestCase(2019, 12, false)]
    public void Then_The_Equals_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1.Equals(f.CalendarPeriod2), (f, r) => r.Should().Be(expected));
    }
    
    [TestCase(2018, 01, true)]
    [TestCase(2018, 02, false)]
    [TestCase(2017, 12, false)]
    [TestCase(2019, 12, false)]
    public void Then_The_Equals_Object_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1.Equals((object)f.CalendarPeriod2), (f, r) => r.Should().Be(expected));
    }
    
    [TestCase(2018, 01, false)]
    [TestCase(2018, 02, true)]
    [TestCase(2017, 12, true)]
    [TestCase(2019, 12, true)]
    public void Then_The_Not_Equals_Operator_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1 != f.CalendarPeriod2, (f, r) => r.Should().Be(expected));
    }
    
    [TestCase(2018, 01, true)]
    [TestCase(2018, 02, false)]
    [TestCase(2017, 12, false)]
    [TestCase(2019, 12, false)]
    public void Then_The_GetHasCode_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1.GetHashCode() == f.CalendarPeriod2.GetHashCode(), (f, r) => r.Should().Be(expected));
    }
    
    [TestCase(2018, 01, true)]
    [TestCase(2018, 02, true)]
    [TestCase(2017, 12, false)]
    [TestCase(2019, 12, true)]
    public void Then_The_Greater_Than_Or_Equals_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1 >= f.CalendarPeriod2, (f, r) => r.Should().Be(expected));
    }

    [TestCase(2018, 01, false)]
    [TestCase(2018, 02, true)]
    [TestCase(2017, 12, false)]
    [TestCase(2019, 12, true)]
    public void Then_The_Greater_Than_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1 > f.CalendarPeriod2, (f, r) => r.Should().Be(expected));
    }

    [TestCase(2018, 01, true)]
    [TestCase(2018, 02, false)]
    [TestCase(2017, 12, true)]
    [TestCase(2016, 12, true)]
    public void Then_The_Less_Than_Or_Equals_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1 <= f.CalendarPeriod2, (f, r) => r.Should().Be(expected));
    }

    [TestCase(2018, 01, false)]
    [TestCase(2018, 02, false)]
    [TestCase(2017, 12, true)]
    [TestCase(2016, 12, true)]
    public void Then_The_Less_Than_Comparison_Is_Correct(int year, int month, bool expected)
    {
        Test(f => f.SetCalendarPeriod1(year, month), f => f.CalendarPeriod1 < f.CalendarPeriod2, (f, r) => r.Should().Be(expected));
    }

    [TestCase("2018-1", "2017-12", true)]
    [TestCase("2018-1", "2018-2", true)]
    [TestCase("2018-8", "2018-9", true)]
    [TestCase("2018-4", "2018-5", false)]
    [TestCase("2017-3", "2018-5", false)]
    public void Then_The_Tax_Years_Are_Compared_Correctly_From_Transaction_Dates(string start, string end, bool expected)
    {
        //Arrange
        var startDate = new CalendarPeriod(Convert.ToInt32(start.Split('-')[0]), Convert.ToInt32(start.Split('-')[1]));
        var endDate = new CalendarPeriod(Convert.ToInt32(end.Split('-')[0]), Convert.ToInt32(end.Split('-')[1]));
        
        //Act
        var actual = startDate.AreSameTaxYear(endDate);

        //Assert
        actual.Should().Be(expected);
    }
}

public class WhenComparingCalendarPeriodsFixture
{
    public CalendarPeriod CalendarPeriod1 { get; set; }
    public CalendarPeriod CalendarPeriod2 { get; set; }

    public WhenComparingCalendarPeriodsFixture()
    {
        CalendarPeriod2 = new CalendarPeriod(2018, 01);
    }

    public void SetCalendarPeriod1(int year, int month)
    {
        CalendarPeriod1 = new CalendarPeriod(year, month);
    }
}