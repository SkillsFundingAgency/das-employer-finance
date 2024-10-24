using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountLevyControllerTests;

[TestFixture]
public class WhenIGetLevyForAnAccount : AccountLevyControllerTests
{
    [Test]
    public async Task ThenTheLevyIsReturned()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var levyResponse = new GetLevyDeclarationResponse { Declarations = LevyDeclarationItems.Create(12334, "abc123") };
        Mediator.Setup(x => x.Send(It.Is<GetLevyDeclarationRequest>(q => q.HashedAccountId == hashedAccountId), It.IsAny<CancellationToken>())).ReturnsAsync(levyResponse);           

        //Act
        var response = await Controller.Index(hashedAccountId);

        //Assert
        response.Should().NotBeNull(); 
        response.Should().BeOfType<OkObjectResult>();
        var model = ((OkObjectResult)response).Value as List<LevyDeclaration>;

        model?.Should().NotBeNull();
        model?.TrueForAll(x => x.HashedAccountId == hashedAccountId).Should().BeTrue();
        levyResponse.Declarations?.Should().BeEquivalentTo(model, options => options.Excluding(x => x.HashedAccountId).Excluding(x => x.PayeSchemeReference));
        (model?[0].PayeSchemeReference == levyResponse.Declarations[0].EmpRef).Should().BeTrue();
    }
}