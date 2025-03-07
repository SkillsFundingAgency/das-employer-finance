using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers;
public class ErrorControllerTests
{
    [Test, MoqAutoData]
    public void NotFound_Should_Return_404(
        [Greedy] ErrorController controller)
    {
        // Act
        var result = controller.Error(404) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result.ViewName.Should().Be("404");
    }

    [Test, MoqAutoData]
    public void InvalidStatus_Should_Return_Generic_Error_Page(
        [Greedy] ErrorController controller)
    {
        // Act
        var result = controller.Error(999) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result.ViewName.Should().BeNull();
    }
}
