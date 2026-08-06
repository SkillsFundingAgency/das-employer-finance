using System.ComponentModel.DataAnnotations;
using AutoFixture.NUnit4;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.UserControllerTests;

public class WhenUpsertingUser
{
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Is_Called_And_Ok_Returned(
        UpsertRegisteredUserCommand request,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] UserController controller)
    {
        var actual = await controller.Upsert(request);
        
        actual.Should().NotBeNull();
        var actualResult = actual as OkResult;
        actualResult.Should().NotBeNull();
        mediator.Verify(x=>x.Send(request, CancellationToken.None));
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Is_Called_And_InternalServerError_Returned_When_Error(
        UpsertRegisteredUserCommand request,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] UserController controller)
    {
        mediator.Setup(x => x.Send(It.IsAny<UpsertRegisteredUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());
        
        var actual = await controller.Upsert(request);
        
        actual.Should().NotBeNull();
        var actualResult = actual as StatusCodeResult;
        actualResult.Should().NotBeNull();
        actualResult.StatusCode.Should().Be(500);
    }

    [Test, MoqAutoData]
    public async Task Then_ValidationException_Is_Rethrown(
        UpsertRegisteredUserCommand request,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] UserController controller)
    {
        var validationResult = new ValidationResult(
            "Validation failed",
            ["EmailAddress|EmailAddress has not been supplied"]);

        mediator.Setup(x => x.Send(It.IsAny<UpsertRegisteredUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationResult, null, null));

        var act = () => controller.Upsert(request);

        await act.Should().ThrowAsync<ValidationException>();
    }
}