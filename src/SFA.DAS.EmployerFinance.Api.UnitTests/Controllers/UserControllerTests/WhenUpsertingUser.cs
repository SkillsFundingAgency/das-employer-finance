using AutoFixture.NUnit3;
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
        
        Assert.IsNotNull(actual);
        var actualResult = actual as OkResult;
        Assert.IsNotNull(actualResult);
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
        
        Assert.IsNotNull(actual);
        var actualResult = actual as StatusCodeResult;
        Assert.IsNotNull(actualResult);
        actualResult.StatusCode.Should().Be(500);
    }
}