using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndPeriod;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountLevyControllerTests;

public class WhenIGetLevyForAnAccountAndPeriod : AccountLevyControllerTests
{
    [Test]
    public async Task ThenTheLevyIsReturned()
    {            
        //Arrange
        var hashedAccountId = "ABC123";
        var payrollYear = "2017-18";
        short payrollMonth = 5;
        var levyResponse = new GetLevyDeclarationsByAccountAndPeriodResponse { Declarations = LevyDeclarationItems.Create(12334, "abc123") };
        Mediator.Setup(
                x => x.Send(It.Is<GetLevyDeclarationsByAccountAndPeriodRequest>(q => q.HashedAccountId == hashedAccountId && q.PayrollYear == payrollYear && q.PayrollMonth == payrollMonth), It.IsAny<CancellationToken>()))
            .ReturnsAsync(levyResponse);            

        //Act
        var response = await Controller.GetLevy(hashedAccountId, payrollYear, payrollMonth);

        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        var model = ((OkObjectResult)response).Value as List<LevyDeclaration>;

        model?.Should().NotBeNull();
        model?.TrueForAll(x => x.HashedAccountId == hashedAccountId).Should().BeTrue();
        levyResponse.Declarations.Should().BeEquivalentTo(model, options => options.Excluding(x => x.HashedAccountId).Excluding(x => x.PayeSchemeReference));

        (model?[0].PayeSchemeReference == levyResponse.Declarations[0].EmpRef).Should().BeTrue();
    }
}