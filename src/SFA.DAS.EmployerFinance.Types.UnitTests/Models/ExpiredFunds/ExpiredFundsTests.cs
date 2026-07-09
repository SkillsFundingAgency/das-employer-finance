using NUnit.Framework;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Types.UnitTests.Models;

[Parallelizable]
public class WhenCalculatingExpiringFunds
{
    private ExpiredFunds _expiredFunds = new();
    private Dictionary<CalendarPeriod, decimal> _fundsIn = new();

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
    public void Then_An_Exception_Is_Returned_When_Params_Are_Not_Supplied()
    {
        var actualFundsInException = Assert.Throws<ArgumentNullException>(() =>
            _expiredFunds.GetExpiringFunds(null, new Dictionary<CalendarPeriod, decimal>(),
                new Dictionary<CalendarPeriod, decimal>(),24));
        Assert.That(actualFundsInException.Message.Contains("fundsIn"), Is.True);
        var actualFundsOutException = Assert.Throws<ArgumentNullException>(() =>
            _expiredFunds.GetExpiringFunds(new Dictionary<CalendarPeriod, decimal>(), null,
                new Dictionary<CalendarPeriod, decimal>(),24));
        Assert.That(actualFundsOutException.Message.Contains("fundsOut"), Is.True);
    }

    [Test]
    public void Then_If_There_Are_No_Funds_In_Then_There_Is_No_Expiry()
    {
        //Act
        var actual = _expiredFunds.GetExpiringFunds(new Dictionary<CalendarPeriod, decimal>(), new Dictionary<CalendarPeriod, decimal>(), null, 3);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(0, Is.EqualTo( actual.Count));
    }

    [Test]
    public void Then_Expired_Funds_Are_Returned_If_There_Are_Funds_In_Depending_On_The_Expiry_Period()
    {
        //Arrange
        var expiryPeriod = 1;

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, new Dictionary<CalendarPeriod, decimal>(), null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(10, Is.EqualTo(actual.First().Value));
        Assert.That(5,  Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_If_I_Have_Funds_In_They_Are_Taken_Off_My_Expiry_Amount()
    {
        //Arrange
        var expiryPeriod = 2;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(0, Is.EqualTo(actual.First().Value));
        Assert.That(9, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(5, Is.EqualTo(actual.Last().Value));
    }


    [Test]
    public void Then_If_I_Have_Refunds_In_They_Are_Taken_Off_My_Expiry_Amount()
    {
        //Arrange
        var expiryPeriod = 3;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), -5}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(5, Is.EqualTo(actual.First().Value));
        Assert.That(9, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(8, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(5, Is.EqualTo(actual.Last().Value));
    }


    [Test]
    public void Then_If_I_Have_Multiple_Refunds_In_They_Are_Taken_Off_My_Expiry_Amount()
    {
        //Arrange
        var expiryPeriod = 3;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), -5},
            {new CalendarPeriod(2018, 12), -2}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(7, Is.EqualTo(actual.First().Value));
        Assert.That(9, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(8, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(5, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_The_Expiry_Date_Returned_Is_Correct_Based_On_The_Expiry_Period()
    {
        //Arrange
        var expiryPeriod = 2;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(2018, Is.EqualTo(actual.First().Key.Year));
        Assert.That(12, Is.EqualTo(actual.First().Key.Month));
        Assert.That(2019, Is.EqualTo(actual.Skip(1).First().Key.Year));
        Assert.That(1, Is.EqualTo(actual.Skip(1).First().Key.Month));
    }


    [Test]
    public void Then_If_I_Have_More_Funds_In_They_Are_Taken_Off_My_Expiry_Amounts_For_Multiple_Months()
    {
        //Arrange
        var expiryPeriod = 2;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 12}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(0, Is.EqualTo(actual.First().Value));
        Assert.That(7, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(5, Is.EqualTo(actual.Last().Value));
    }


    [Test]
    public void Then_If_I_Have_A_Large_Carry_Over_Funds_In_They_Are_Taken_Off_My_Expiry_Amounts_For_Multiple_Months_And_Expiry_Periods()
    {
        //Arrange
        var fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), 9},
            {new CalendarPeriod(2018, 12), 8},
            {new CalendarPeriod(2019, 1), 5},
            {new CalendarPeriod(2019, 2), 5},
            {new CalendarPeriod(2019, 3), 5},
            {new CalendarPeriod(2020, 3), 5}
        };
        var expiryPeriod = 2;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 11), 120}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(7, Is.EqualTo(actual.Count));
        Assert.That(actual.All(c=>c.Value.Equals(0)), Is.True);
    }

    [Test]
    public void Then_My_Funds_Out_Are_Taken_Off_The_Correct_Funds_In_Period()
    {
        //Arrange
        var expiryPeriod = 2;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 1), 12}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(10, Is.EqualTo(actual.First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(5, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(5, Is.EqualTo(actual.Last().Value));
    }


    [Test]
    public void Then_If_I_Have_Multiple_Funds_Out_Are_Taken_Off_The_Correct_Funds_In_Period()
    {
        //Arrange
        var expiryPeriod = 2;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 1), 12},
            {new CalendarPeriod(2019, 2), 3}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(10, Is.EqualTo(actual.First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(2, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(5, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_If_I_Have_Multiple_Funds_Out_Not_For_The_Valid_Period_Then_Funds_Expire()
    {
        //Arrange
        var fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 1), 5},
            { new CalendarPeriod(2018, 11), 9},
            {new CalendarPeriod(2018, 12), 8},
            {new CalendarPeriod(2018, 10), 10}
        };
        var expiryPeriod = 2;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 4), 12},
            {new CalendarPeriod(2019, 5), 3}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(fundsIn, fundsOut, null,
            expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(10, Is.EqualTo(actual.First().Value));
        Assert.That(9, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(8, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(5, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_If_I_Have_Seasonal_Values_The_Funds_Expire_Correctly()
    {
        //Arrange
        var fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 9), 10},
            {new CalendarPeriod(2018, 12), 8},
            {new CalendarPeriod(2019, 1), 5},
            {new CalendarPeriod(2019, 4), 2}
        };
        var expiryPeriod = 2;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 1), 12},
            {new CalendarPeriod(2019, 2), 3}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(fundsIn, fundsOut, null, expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(10, Is.EqualTo(actual.First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(3).First().Value));
        Assert.That(0, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_If_I_Have_Adjustments_On_My_Funds_In_It_Is_Applied_In_Descending_Order_To_The_Funds_In()
    {
        //Arrange
        var fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), 9},
            {new CalendarPeriod(2018, 12), -10},
            {new CalendarPeriod(2019, 1), 5},
            {new CalendarPeriod(2019, 2), -5},
        };
        var expiryPeriod = 6;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {

        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(fundsIn, fundsOut, null, expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(5, Is.EqualTo(actual.Count));
        Assert.That(9, Is.EqualTo(actual.First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(3).First().Value));
        Assert.That(0, Is.EqualTo(actual.Last().Value));
    }


    [Test]
    public void Then_If_I_Have_Large_Adjustments_On_My_Funds_In_It_Is_Applied_In_Descending_Order()
    {
        //Arrange
        var fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), 29},
            {new CalendarPeriod(2018, 12), -19},
            {new CalendarPeriod(2019, 1), 5},
            {new CalendarPeriod(2019, 6), -5},
        };
        var expiryPeriod = 24;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {

        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(fundsIn, fundsOut, null, expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(5, Is.EqualTo(actual.Count));
        Assert.That(10, Is.EqualTo(actual.First().Value));
        Assert.That(10, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(3).First().Value));
        Assert.That(0, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_Adjustments_Are_Applied_Over_Multiple_Periods()
    {
        //Arrange
        var fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 12), 10},
            {new CalendarPeriod(2019, 12), -10}
        };
        var expiryPeriod = 12;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {

        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(fundsIn, fundsOut, null, expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(2, Is.EqualTo(actual.Count));
        Assert.That(0, Is.EqualTo(actual.First().Value));
        Assert.That(0, Is.EqualTo(actual.Last().Value));
    }


    [Test]
    public void Then_Adjustments_And_Funds_Out_Are_Applied_In_The_Correct_Periods()
    {
        //Arrange
        var fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 20},
            {new CalendarPeriod(2018, 11), 9},
            {new CalendarPeriod(2018, 12), -5},
            {new CalendarPeriod(2019, 1), 5},
            {new CalendarPeriod(2019, 2), -5},
        };
        var expiryPeriod = 6;
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 1), 12},
            {new CalendarPeriod(2019, 2), 3}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(fundsIn, fundsOut, null, expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(5, Is.EqualTo(actual.Count));
        Assert.That(5, Is.EqualTo(actual.First().Value));
        Assert.That(4, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(3).First().Value));
        Assert.That(0, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_When_I_Already_Have_Expired_Funds_These_Are_Included_In_The_Calculation()
    {
        //Arrange
        var expiryPeriod = 2;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), 9},
            {new CalendarPeriod(2018, 12), 8},
            {new CalendarPeriod(2019, 1), 5}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 11), 12},
            {new CalendarPeriod(2019, 1), 3}
        };
        var expiredFunds = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 12), 9},
            {new CalendarPeriod(2019, 1), 4},
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expiredFunds, expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(4, Is.EqualTo(actual.Count));
        Assert.That(9, Is.EqualTo(actual.First().Value));
        Assert.That(4, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(4, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_When_I_Already_Have_Expired_Funds_And_Adjustments_They_Are_Applied_To_The_None_Expired_FundsIn()
    {
        //Arrange
        var expiryPeriod = 4;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), 9},
            {new CalendarPeriod(2018, 12), 8},
            {new CalendarPeriod(2019, 1), -5},
            {new CalendarPeriod(2019, 2), 5},
            {new CalendarPeriod(2019, 3), -4}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 11), 10}, 
            {new CalendarPeriod(2019, 1), 2} 
        };
        var expiredFunds = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 2), 9},
            {new CalendarPeriod(2019, 3), 4},
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expiredFunds, expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(6, Is.EqualTo(actual.Count));
        Assert.That(9, Is.EqualTo(actual.First().Value));
        Assert.That(4, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(3).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(4).First().Value));
        Assert.That(0, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_When_I_Already_Have_Expired_Funds_And_Adjustments_And_Refunds_They_Are_Applied_To_The_None_Expired_FundsIn()
    {
        //Arrange
        var expiryPeriod = 8;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 10), 10},
            {new CalendarPeriod(2018, 11), 9},
            {new CalendarPeriod(2018, 12), 8},
            {new CalendarPeriod(2019, 1), -26},
            {new CalendarPeriod(2019, 2), 5},
            {new CalendarPeriod(2019, 3), -4},
            {new CalendarPeriod(2019, 4), 5},
            {new CalendarPeriod(2019, 5), 5},
            {new CalendarPeriod(2019, 6), 5}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2018, 11), 1}, 
            {new CalendarPeriod(2018, 12), 1}, 
            {new CalendarPeriod(2019, 1), 1}, 
            {new CalendarPeriod(2019, 2), 1} 
        };
        var expiredFunds = new Dictionary<CalendarPeriod, decimal>
        {
             {new CalendarPeriod(2019, 2), 0},
             {new CalendarPeriod(2019, 3), 0},
             {new CalendarPeriod(2019, 4), 0},
             //{new CalendarPeriod(2019, 5), 0},
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expiredFunds, expiryPeriod);

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(9, Is.EqualTo(actual.Count));
        Assert.That(0, Is.EqualTo(actual.First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(3).First().Value));
        Assert.That(0, Is.EqualTo(actual.Skip(4).First().Value));
        Assert.That(5, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_The_Balance_Is_Correctly_Calculated_Over_A_Large_Expiry_Period()
    {
        //Arrange
        var expiryPeriod = 24;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 5), 13263.8916m},
            {new CalendarPeriod(2017, 6), 11007.99m},
            {new CalendarPeriod(2017, 7), 11554.93m}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 10), 1142.85715m},
        };
        var expired = new Dictionary<CalendarPeriod, decimal>
        {
            //{new CalendarPeriod(2019, 5), 8700} ,
            //{new CalendarPeriod(2019, 6), 13700},
            //{new CalendarPeriod(2019, 7), 12500}
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expired, expiryPeriod);

        //Assert
        Assert.That(11554.93m, Is.EqualTo(actual.Skip(2).First().Value));
    }

    [Test]
    public void Then_The_Next_Months_Expiry_Is_Calculated_Correctly_When_All_Payments_Have_Been_Used_And_There_Is_An_Expiry()
    {
        var expiryPeriod = 4;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 5), 539},
            {new CalendarPeriod(2017, 6), 539},
            {new CalendarPeriod(2017, 7), 539},
            {new CalendarPeriod(2017, 8), 539}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 6), 200},
            {new CalendarPeriod(2017, 7), 200}
        };
        var expired = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017,9),139  }
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expired, expiryPeriod);

        //Assert
        Assert.That(139, Is.EqualTo(actual.First().Value));
        Assert.That(539, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(539, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_The_Next_Months_Expiry_Is_Calculated_Correctly_When_All_Payments_Have_Been_Used_And_There_Is_No_Expiry()
    {
        var expiryPeriod = 4;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 5), 400},
            {new CalendarPeriod(2017, 6), 539},
            {new CalendarPeriod(2017, 7), 539},
            {new CalendarPeriod(2017, 8), 539}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 6), 200},
            {new CalendarPeriod(2017, 7), 200},
            {new CalendarPeriod(2017, 8), 100}
        };
        var expired = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017,9),0  }
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expired, expiryPeriod);

        //Assert
        Assert.That(0, Is.EqualTo(actual.First().Value));
        Assert.That(439, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(539, Is.EqualTo(actual.Last().Value));
    }


    [Test]
    public void Then_The_Next_Months_Expiry_Is_Calculated_Correctly_When_All_Payments_Have_Been_Used_And_There_Is_Expiry()
    {
        var expiryPeriod = 4;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 5), 400},
            {new CalendarPeriod(2017, 6), 539},
            {new CalendarPeriod(2017, 7), 539},
            {new CalendarPeriod(2017, 8), 539}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 6), 100},
            {new CalendarPeriod(2017, 7), 200},
            {new CalendarPeriod(2017, 10), 100},
            {new CalendarPeriod(2017, 12), 100},
        };
        var expired = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017,9),100  },
            {new CalendarPeriod(2017,10),439  }
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expired, expiryPeriod);

        //Assert
        Assert.That(100, Is.EqualTo(actual.First().Value));
        Assert.That(439, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(539, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(439, Is.EqualTo(actual.Last().Value));
    }


    [Test]
    public void Then_The_Next_Months_Expiry_Is_Calculated_Correctly_When_All_Payments_Have_Been_Used_And_There_Are_Multiple_Expiries()
    {
        var expiryPeriod = 4;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 5), 539},
            {new CalendarPeriod(2017, 6), 539},
            {new CalendarPeriod(2017, 7), 539},
            {new CalendarPeriod(2017, 8), 539}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 6), 200},
            {new CalendarPeriod(2017, 7), 200}
        };
        var expired = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017,9),139  },
            {new CalendarPeriod(2017,10),539  }
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expired, expiryPeriod);

        //Assert
        Assert.That(139, Is.EqualTo(actual.First().Value));
        Assert.That(539, Is.EqualTo(actual.Skip(1).First().Value));
        Assert.That(539, Is.EqualTo(actual.Last().Value));
    }

    [Test]
    public void Then_If_There_Has_Been_A_Previous_Adjustment_On_An_Already_Expired_Amount_It_Is_Not_Double_Counted()
    {
        //Arrange
        var expiryPeriod = 5;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 5), 75},
            {new CalendarPeriod(2017, 6), 75},
            {new CalendarPeriod(2017, 7), 75},
            {new CalendarPeriod(2017, 8), 75},
            {new CalendarPeriod(2017, 9), 75},
            {new CalendarPeriod(2017, 10), 75},
            {new CalendarPeriod(2017, 11), 75},
            {new CalendarPeriod(2017, 12), 75}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017,6), 25},
            {new CalendarPeriod(2017,7), 25},
            {new CalendarPeriod(2017,8), 50},
            {new CalendarPeriod(2017,9), -25},
            {new CalendarPeriod(2017,10), 50},
            {new CalendarPeriod(2017,11), 25},
            {new CalendarPeriod(2017,12), 25},
        };
        var expired = new Dictionary<CalendarPeriod, decimal>
        {
             {new CalendarPeriod(2017, 10), 0},
             {new CalendarPeriod(2017, 11), 0}
        };
        
        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expired, expiryPeriod);
        
        //Assert
        Assert.That(50, Is.EqualTo(actual.Skip(2).First().Value));
        Assert.That(75, Is.EqualTo(actual.Skip(3).First().Value));
    }

    [Test]
    public void Then_The_Balance_Is_Correctly_Calculated_Over_A_Large_Expiry_Period_Just_Funds_In()
    {
        //Arrange
        var expiryPeriod = 24;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 5), 539},
            {new CalendarPeriod(2017, 6), 539},
            {new CalendarPeriod(2017, 7), 539},
            {new CalendarPeriod(2017, 8), 539},
            {new CalendarPeriod(2017, 9), 539},
            {new CalendarPeriod(2017, 10), 539},
            {new CalendarPeriod(2017, 11), 539},
            {new CalendarPeriod(2017, 12), 539},
            {new CalendarPeriod(2018, 1), 539},
            {new CalendarPeriod(2018, 2), 539},
            {new CalendarPeriod(2018, 3), (decimal)-323.4},
            {new CalendarPeriod(2018, 4), 539},
            {new CalendarPeriod(2018, 5), (decimal)-323.4},
            {new CalendarPeriod(2018, 6), 539},
            {new CalendarPeriod(2018, 7), 539},
            {new CalendarPeriod(2018, 8), 539},
            {new CalendarPeriod(2018, 9), 539},
            {new CalendarPeriod(2018, 10), 539},
            {new CalendarPeriod(2018, 11), 539},
            {new CalendarPeriod(2018, 12), 539},
            {new CalendarPeriod(2019, 1), (decimal)-323.4}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 1), 0},
            {new CalendarPeriod(2019, 2), 0},
            {new CalendarPeriod(2019, 3), 0},
            {new CalendarPeriod(2019, 4), 0},
            {new CalendarPeriod(2019, 5), 100},
            {new CalendarPeriod(2019, 6), 0} ,
            {new CalendarPeriod(2019, 7), 0},
            {new CalendarPeriod(2019, 8), 0}
        };
        var expired = new Dictionary<CalendarPeriod, decimal>
        {
            
        };

        //Act
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expired, expiryPeriod);

        //Assert
        Assert.That(439, Is.EqualTo(actual.First().Value));
        Assert.That(539, Is.EqualTo(actual.Skip(3).First().Value));
    }
    
    
    [Test]
    public void Then_The_Balance_Is_Correctly_Calculated_Over_A_Large_Expiry_Period_When_Expiry_Has_An_EndDate()
    {
        //Arrange
        var expiryPeriod = 24;
        _fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2017, 5), 539},
            {new CalendarPeriod(2017, 6), 539},
            {new CalendarPeriod(2017, 7), 539},
            {new CalendarPeriod(2017, 8), 539},
            {new CalendarPeriod(2017, 9), 539},
            {new CalendarPeriod(2017, 10), 539},
            {new CalendarPeriod(2017, 11), 539},
            {new CalendarPeriod(2017, 12), 539},
            {new CalendarPeriod(2018, 1), 539},
            {new CalendarPeriod(2018, 2), 539},
            {new CalendarPeriod(2018, 3), (decimal)-323.4},
            {new CalendarPeriod(2018, 4), 539},
            {new CalendarPeriod(2018, 5), (decimal)-323.4},
            {new CalendarPeriod(2018, 6), 539},
            {new CalendarPeriod(2018, 7), 539},
            {new CalendarPeriod(2018, 8), 539},
            {new CalendarPeriod(2018, 9), 539},
            {new CalendarPeriod(2018, 10), 539},
            {new CalendarPeriod(2018, 11), 539},
            {new CalendarPeriod(2018, 12), 539},
            {new CalendarPeriod(2019, 1), (decimal)-323.4}
        };
        var fundsOut = new Dictionary<CalendarPeriod, decimal>
        {
            {new CalendarPeriod(2019, 1), 0},
            {new CalendarPeriod(2019, 2), 0},
            {new CalendarPeriod(2019, 3), 0},
            {new CalendarPeriod(2019, 4), 0},
            {new CalendarPeriod(2019, 5), 100},
            {new CalendarPeriod(2019, 6), 0} ,
            {new CalendarPeriod(2019, 7), 0},
            {new CalendarPeriod(2019, 8), 0}
        };
        var expired = new Dictionary<CalendarPeriod, decimal>
        {
            
        };

        //Act 5-2018
        var actual = _expiredFunds.GetExpiringFunds(_fundsIn, fundsOut, expired, expiryPeriod, new DateTime(2018, 5, 1));

        //Assert
        Assert.That(439, Is.EqualTo(actual.First().Value));
        Assert.That(539, Is.EqualTo(actual.Skip(3).First().Value));
        Assert.That(actual.Skip(12).Select(c=>c.Value).Sum(), Is.EqualTo(0));
    }
}