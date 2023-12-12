using AutoFixture.NUnit3;
using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Models.UserAccounts;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Handlers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.AppStart;

public class WhenPopulatingAccountClaims
{
    [Test, MoqAutoData]
    public async Task Then_The_Claims_Are_Populated_For_Gov_User(
        string nameIdentifier,
        string idamsIdentifier,
        string emailAddress,
        EmployerUserAccounts accountData,
        [Frozen] Mock<IUserAccountService> accountService,
        [Frozen] Mock<IOptions<EmployerFinanceWebConfiguration>> forecastingConfiguration,
        EmployerAccountPostAuthenticationClaimsHandler handler)
    {
        accountData.IsSuspended = false;
        forecastingConfiguration.Object.Value.UseGovSignIn = true;
        var tokenValidatedContext = ArrangeTokenValidatedContext(nameIdentifier, idamsIdentifier, emailAddress);
        accountService.Setup(x => x.GetUserAccounts(nameIdentifier, emailAddress)).ReturnsAsync(accountData);

        var actual = await handler.GetClaims(tokenValidatedContext);

        accountService.Verify(x => x.GetUserAccounts(nameIdentifier, emailAddress), Times.Once);
        accountService.Verify(x => x.GetUserAccounts(idamsIdentifier, emailAddress), Times.Never);
        actual.Should().ContainSingle(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
        
        var actualClaimValue = actual.First(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)).Value;
        JsonConvert.SerializeObject(accountData.EmployerAccounts.ToDictionary(k => k.AccountId)).Should().Be(actualClaimValue);
        
        actual.First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier)).Value.Should().Be(accountData.EmployerUserId);
        actual.First(c => c.Type.Equals(EmployerClaims.IdamsUserEmailClaimTypeIdentifier)).Value.Should().Be(emailAddress);
        actual.First(c => c.Type.Equals(DasClaimTypes.GivenName)).Value.Should().Be(accountData.FirstName);
        actual.First(c => c.Type.Equals(DasClaimTypes.FamilyName)).Value.Should().Be(accountData.LastName);
        actual.First(c => c.Type.Equals(EmployerClaims.IdamsUserDisplayNameClaimTypeIdentifier)).Value.Should().Be($"{accountData.FirstName} {accountData.LastName}");
        actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision))?.Value.Should().BeNullOrEmpty();
    }

    [Test, MoqAutoData]
    public async Task Then_The_Claims_Are_Populated_For_Gov_User_And_Suspended_Flag_Set(
        string nameIdentifier,
        string idamsIdentifier,
        string emailAddress,
        EmployerUserAccounts accountData,
        [Frozen] Mock<IUserAccountService> accountService,
        [Frozen] Mock<IOptions<EmployerFinanceWebConfiguration>> forecastingConfiguration,
        EmployerAccountPostAuthenticationClaimsHandler handler)
    {
        accountData.IsSuspended = true;
        forecastingConfiguration.Object.Value.UseGovSignIn = true;
        var tokenValidatedContext = ArrangeTokenValidatedContext(nameIdentifier, idamsIdentifier, emailAddress);
        accountService.Setup(x => x.GetUserAccounts(nameIdentifier, emailAddress)).ReturnsAsync(accountData);

        var actual = await handler.GetClaims(tokenValidatedContext);

        actual.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthorizationDecision)).Value.Should().Be("Suspended");
    }

    [Test, MoqAutoData]
    public async Task Then_The_Claims_Are_Populated_For_EmployerUsers_User(
        string nameIdentifier,
        string idamsIdentifier,
        EmployerUserAccounts accountData,
        [Frozen] Mock<IUserAccountService> accountService,
        [Frozen] Mock<IOptions<EmployerFinanceWebConfiguration>> financeConfiguration,
        EmployerAccountPostAuthenticationClaimsHandler handler)
    {
        var tokenValidatedContext = ArrangeTokenValidatedContext(nameIdentifier, idamsIdentifier, string.Empty);
        accountService.Setup(x => x.GetUserAccounts(idamsIdentifier, "")).ReturnsAsync(accountData);
        financeConfiguration.Object.Value.UseGovSignIn = false;

        var actual = await handler.GetClaims(tokenValidatedContext);

        accountService.Verify(x => x.GetUserAccounts(nameIdentifier, string.Empty), Times.Never);
        accountService.Verify(x => x.GetUserAccounts(idamsIdentifier, string.Empty), Times.Once);
        actual.Should().ContainSingle(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
        var actualClaimValue = actual.First(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)).Value;
        JsonConvert.SerializeObject(accountData.EmployerAccounts.ToDictionary(k => k.AccountId)).Should().Be(actualClaimValue);
        actual.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier)).Should().NotBeNull();
        actual.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserDisplayNameClaimTypeIdentifier)).Should().BeNull();
    }

    private static TokenValidatedContext ArrangeTokenValidatedContext(string nameIdentifier, string idamsIdentifier, string emailAddress, Mock<IAuthenticationOrchestrator> authenticationOrchestrator = null)
    {
        var identity = new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, nameIdentifier),
            new(EmployerClaims.IdamsUserIdClaimTypeIdentifier, idamsIdentifier),
            new(ClaimTypes.Email, emailAddress),
            new(EmployerClaims.IdamsUserEmailClaimTypeIdentifier, emailAddress)
        });

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(identity));
        var defaultHttpContext = new DefaultHttpContext();
        var mockServiceProvider = new Mock<IServiceProvider>();
        authenticationOrchestrator ??= new Mock<IAuthenticationOrchestrator>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAuthenticationOrchestrator))).Returns(authenticationOrchestrator.Object);
        
        defaultHttpContext.RequestServices = mockServiceProvider.Object;
        return new TokenValidatedContext(defaultHttpContext, new AuthenticationScheme(",", "", typeof(TestAuthHandler)),
            new OpenIdConnectOptions(), Mock.Of<ClaimsPrincipal>(), new AuthenticationProperties())
        {
            Principal = claimsPrincipal
        };
    }


    private class TestAuthHandler : IAuthenticationHandler
    {
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }
    }
}