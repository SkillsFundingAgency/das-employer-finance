using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.ViewModels;

public class WhenCheckingIfTheEstimatesEnterCoInvestment
{
    [Test]
    public void ThenTheBannerIsNotShownWhenThereIsNoDateTheEstimateEntersCoInvestment()
    {
        var model = new EstimatesViewModel { DateEstimateEntersCoinvestment = null };

        model.ShowCoInvestmentBanner.Should().BeFalse();
        model.CoInvestmentStartLabel.Should().BeNull();
    }

    [Test]
    public void ThenTheBannerIsShownWithTheMonthTheEstimateEntersCoInvestment()
    {
        var model = new EstimatesViewModel { DateEstimateEntersCoinvestment = new DateTime(2027, 3, 1) };

        model.ShowCoInvestmentBanner.Should().BeTrue();
        model.CoInvestmentStartLabel.Should().Be("March 2027");
    }

    [Test]
    public void ThenTheBannerIsShownWhenTheDateIsAfterTheMonthsShown()
    {
        var model = new EstimatesViewModel
        {
            DateEstimateEntersCoinvestment = new DateTime(2027, 3, 1),
            MonthEstimates = [new MonthEstimateViewModel { Period = "November 2026" }]
        };

        model.ShowCoInvestmentBanner.Should().BeTrue();
    }
}
