using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.UserAccounts;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Authorization;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.Testing.AutoFixture;
using EmployerClaims = SFA.DAS.GovUK.Auth.Employer.EmployerClaims;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.AppStart;

public class WhenHandlingEmployerAccountAuthorization
{
    [Test, MoqAutoData]
    public async Task ThenReturnsFalseWhenUserClaimsAreEmpty(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();

        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity(new List<Claim>())]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_True_If_Employer_Is_Authorized_For_Owner_Role(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        EmployerUserAccountItem serviceResponse,
        [Frozen] Mock<IAccountClaimsService> associatedAccountsService,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        serviceResponse.AccountId = employerIdentifier.AccountId.ToUpper();
        serviceResponse.Role = employerIdentifier.Role;
        employerIdentifier.AccountId = employerIdentifier.AccountId;
        
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        var accounts = new List<EmployerUserAccountItem>
        {
            serviceResponse
        };

        var accountsDictionary = accounts.ToDictionary(x => x.AccountId);

        associatedAccountsService.Setup(x => x.GetAssociatedAccounts(false)).ReturnsAsync(accountsDictionary);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeTrue();
    }

    [Test]
    [MoqInlineAutoData("Transactor")]
    [MoqInlineAutoData("Viewer")]
    public async Task Then_Returns_False_If_Employer_Is_Authorized_For_Role_That_Is_Not_Owner(
        string role,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = role;
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeFalse();
    }

    [Test]
    [MoqInlineAutoData("Transactor")]
    [MoqInlineAutoData("Viewer")]
    public async Task Then_Viewer_And_Transactor_Are_Allowed_For_All_Roles(
        string role,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        EmployerUserAccountItem serviceResponse,
        [Frozen] Mock<IAccountClaimsService> associatedAccountsService,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = role;
        serviceResponse.Role = role;
        serviceResponse.AccountId = employerIdentifier.AccountId.ToUpper();
        employerIdentifier.AccountId =  serviceResponse.AccountId;
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        var accounts = new List<EmployerUserAccountItem>
        {
            serviceResponse
        };

        var accountsDictionary = accounts.ToDictionary(x => x.AccountId);

        associatedAccountsService.Setup(x => x.GetAssociatedAccounts(false)).ReturnsAsync(accountsDictionary);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeTrue();
    }

    [Test]
    [MoqAutoData]
    public async Task Then_Returns_False_For_Employer_With_Role_None(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "None";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }


    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_Employer_Is_Not_Authorized(
        string accountId,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_User_EmployerAccounts_Should_Be_Retrieved_From_AssociatedAccountsService(
        EmployerIdentifier employerIdentifier,
        string userId,
        string email,
        EmployerUserAccountItem serviceResponse,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IAccountClaimsService> associatedAccountsService,
        EmployerAccountAuthorisationHandler authHandler
    )
    {
        //Arrange
        serviceResponse.AccountId = serviceResponse.AccountId.ToUpper();
        employerIdentifier.Role = "Viewer";
        employerIdentifier.AccountId = serviceResponse.AccountId;

        var claimsPrinciple = new ClaimsPrincipal([
            new ClaimsIdentity([
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId)
            ])
        ]);

        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var accounts = new List<EmployerUserAccountItem>
        {
            serviceResponse
        };

        var accountsDictionary = accounts.ToDictionary(x => x.AccountId);

        associatedAccountsService.Setup(x => x.GetAssociatedAccounts(false)).ReturnsAsync(accountsDictionary);

        //Act
        await authHandler.IsEmployerAuthorised(context, allowAllUserRoles: false);

        //Assert
        associatedAccountsService.Verify(x => x.GetAssociatedAccounts(false), Times.Once);
        associatedAccountsService.Verify(x => x.GetAssociatedAccounts(true), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Then_If_Not_In_Context_Claims_EmployerAccountService_Checked_And_False_Returned_If_Not_Exists(
        string accountId,
        string userId,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        EmployerUserAccountItem serviceResponse,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IAccountClaimsService> associatedAccountsService,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        serviceResponse.AccountId = serviceResponse.AccountId.ToUpper();
        serviceResponse.Role = "Owner";

        var userClaim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId);
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([employerAccountClaim, userClaim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        var accounts = new List<EmployerUserAccountItem>
        {
            serviceResponse
        };

        var accountsDictionary = accounts.ToDictionary(x => x.AccountId);

        associatedAccountsService.Setup(x => x.GetAssociatedAccounts(false)).ReturnsAsync(accountsDictionary);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_User_EmployerAccounts_Should_Be_Retrieved_From_AssociatedAccountsService_With_Force_Refresh_When_AssociatedAccount_Is_Missing(
        EmployerIdentifier employerIdentifier,
        string userId,
        string email,
        EmployerUserAccountItem serviceResponse,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        Mock<ILogger<EmployerAccountAuthorisationHandler>> logger,
        [Frozen] Mock<IAccountClaimsService> associatedAccountsService,
        EmployerAccountAuthorisationHandler authHandler
    )
    {
        //Arrange
        employerIdentifier.Role = "Viewer";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var claimsPrinciple = new ClaimsPrincipal([
            new ClaimsIdentity([
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId)
            ])
        ]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };

        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var accounts = new List<EmployerUserAccountItem>
        {
            serviceResponse
        };

        var accountsDictionary = accounts.ToDictionary(x => x.AccountId);

        associatedAccountsService.Setup(x => x.GetAssociatedAccounts(false)).ReturnsAsync(() => new Dictionary<string, EmployerUserAccountItem>());
        associatedAccountsService.Setup(x => x.GetAssociatedAccounts(true)).ReturnsAsync(accountsDictionary);

        //Act
        await authHandler.IsEmployerAuthorised(context, allowAllUserRoles: false);

        //Assert
        associatedAccountsService.Verify(x => x.GetAssociatedAccounts(false), Times.Once);
        associatedAccountsService.Verify(x => x.GetAssociatedAccounts(true), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_Employer_Is_Authorized_But_Has_Invalid_Role_But_Should_Allow_All_known_Roles(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Viewer-Owner-Transactor";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }


    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_AccountId_Not_In_Url(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Clear();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_No_Matching_AccountIdentifier_Claim_Found(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Viewer-Owner-Transactor";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim("SomeOtherClaim", JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_No_Matching_NameIdentifier_Claim_Found_For_GovSignIn(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IOptions<EmployerFinanceWebConfiguration>> forecastingConfiguration,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Viewer-Owner-Transactor";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim("SomeOtherClaim", JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_The_Claim_Cannot_Be_Deserialized(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerIdentifier));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }
}