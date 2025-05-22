using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransfersControllerTests;

public class TransfersControllerTests
{
    [Test, MoqAutoData]
    public void FinancialBreakdown_ShouldRedirectToErrorIndexWith404Status(
            [Greedy] TransfersController controller)
    {
        // Act
        var result = controller.FinancialBreakdown() as RedirectToActionResult;

        // Assert
        result.Should().NotBeNull();
        result.ActionName.Should().Be("Index");
        result.ControllerName.Should().Be("Error");
        result.RouteValues.Should().ContainKey("statusCode");
        result.RouteValues["statusCode"].Should().Be(404);
    }
}