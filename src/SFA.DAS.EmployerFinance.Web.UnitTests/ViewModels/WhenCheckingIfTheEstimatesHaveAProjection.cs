using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.ViewModels;

public class WhenCheckingIfTheEstimatesHaveAProjection
{
    [Test]
    public void ThenThereIsNoProjectionWhenItHasNeverBeenImported()
    {
        var model = new EstimatesViewModel { LastUpdatedUtc = DateTime.MinValue };

        model.HasProjection.Should().BeFalse();
    }

    [Test]
    public void ThenThereIsNoProjectionWhenTheLastUpdatedDateIsNotSet()
    {
        new EstimatesViewModel().HasProjection.Should().BeFalse();
    }

    [Test]
    public void ThenThereIsNoProjectionWhenTheImportDateIsTheMinimumRegardlessOfKind()
    {
        var model = new EstimatesViewModel { LastUpdatedUtc = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc) };

        model.HasProjection.Should().BeFalse();
    }

    [Test]
    public void ThenThereIsAProjectionOnceItHasBeenImported()
    {
        var model = new EstimatesViewModel { LastUpdatedUtc = new DateTime(2026, 11, 24, 9, 30, 0, DateTimeKind.Utc) };

        model.HasProjection.Should().BeTrue();
    }
}
