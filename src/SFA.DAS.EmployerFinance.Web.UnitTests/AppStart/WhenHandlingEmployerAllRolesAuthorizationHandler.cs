using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http.Features;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.UserAccounts;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.AppStart;

public class WhenHandlingEmployerAllRolesAuthorizationHandler
{
    [Test, MoqAutoData]
    public async Task Then_Returns_Succeeded_If_Employer_Is_Authorized_For_Owner_Role(
        string role,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IEmployerAccountAuthorisationHandler> handler,
        EmployerAccountOwnerAuthorizationHandler authorizationHandler)
    {
        //Arrange
        var context = new AuthorizationHandlerContext(new [] {ownerRequirement}, new ClaimsPrincipal(), null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        handler.Setup(x => x.IsEmployerAuthorised(context, false)).ReturnsAsync(true);
        
        //Act
        await authorizationHandler.HandleAsync(context);

        //Assert
        context.HasSucceeded.Should().BeTrue();
    }
    [Test, MoqAutoData]
    public async Task Then_Returns_Failed_If_Employer_Is_Not_Authorized_For_Owner_Role(
        string role,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IEmployerAccountAuthorisationHandler> handler,
        EmployerAccountOwnerAuthorizationHandler authorizationHandler)
    {
        //Arrange
        var context = new AuthorizationHandlerContext(new [] {ownerRequirement}, new ClaimsPrincipal(), null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        handler.Setup(x => x.IsEmployerAuthorised(context, false)).ReturnsAsync(false);
        
        //Act
        await authorizationHandler.HandleAsync(context);

        //Assert
        context.HasSucceeded.Should().BeFalse();
    }
    
}