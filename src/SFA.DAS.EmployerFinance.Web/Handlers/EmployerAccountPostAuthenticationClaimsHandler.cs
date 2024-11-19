using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.Handlers;

public class EmployerAccountPostAuthenticationClaimsHandler(
    IUserAccountService userAccountService,
    ILogger<EmployerAccountPostAuthenticationClaimsHandler> logger)
    : ICustomClaims
{
    public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
    {
        logger.LogInformation("Updating finance claims");
        var claims = new List<Claim>();

        var userId = tokenValidatedContext.Principal.Claims
                .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                .Value;
        var email = tokenValidatedContext.Principal.Claims
            .First(c => c.Type.Equals(ClaimTypes.Email))
            .Value;
        claims.Add(new Claim(EmployerClaims.IdamsUserEmailClaimTypeIdentifier, email));
                
        var result = await userAccountService.GetUserAccounts(userId, email);

        var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
        logger.LogInformation("CF: Accounts {AccountsAsJson}", accountsAsJson);
        var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
        claims.Add(associatedAccountsClaim);

        if (result.IsSuspended)
        {
            claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
        }

        claims.Add(new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, result.EmployerUserId));
        
        if (!string.IsNullOrEmpty(result.FirstName) && !string.IsNullOrEmpty(result.LastName))
        {
            claims.Add(new Claim(DasClaimTypes.GivenName, result.FirstName));
            claims.Add(new Claim(DasClaimTypes.FamilyName, result.LastName));
            claims.Add(new Claim(EmployerClaims.IdamsUserDisplayNameClaimTypeIdentifier, $"{result.FirstName} {result.LastName}"));    
        }

        return claims;
    }
}