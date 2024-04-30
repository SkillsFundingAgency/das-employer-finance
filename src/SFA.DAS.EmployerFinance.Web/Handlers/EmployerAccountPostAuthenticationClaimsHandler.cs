using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.Handlers;

public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
{
    private readonly EmployerFinanceWebConfiguration _employerFinanceConfiguration;
    private readonly IUserAccountService _userAccountService;
    private readonly ILogger<EmployerAccountPostAuthenticationClaimsHandler> _logger;

    public EmployerAccountPostAuthenticationClaimsHandler(
        IUserAccountService userAccountService, 
        IOptions<EmployerFinanceWebConfiguration> employerFinanceConfiguration,
        ILogger<EmployerAccountPostAuthenticationClaimsHandler> logger
    )
    {
        _userAccountService = userAccountService;
        _logger = logger;
        _employerFinanceConfiguration = employerFinanceConfiguration.Value;
    }

    public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
    {
        _logger.LogInformation("Updating finance claims");
        var claims = new List<Claim>();
        
        string userId;
        string email;
            
        if (_employerFinanceConfiguration.UseGovSignIn)
        {
            userId = tokenValidatedContext.Principal.Claims
                .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                .Value;
            email = tokenValidatedContext.Principal.Claims
                .First(c => c.Type.Equals(ClaimTypes.Email))
                .Value;
            claims.Add(new Claim(EmployerClaims.IdamsUserEmailClaimTypeIdentifier, email));
        }
        else
        {
            userId = tokenValidatedContext.Principal.Claims
                .First(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier))
                .Value;

            email = tokenValidatedContext.Principal.Claims
                .First(c => c.Type.Equals(EmployerClaims.IdamsUserEmailClaimTypeIdentifier)).Value;

            claims.AddRange(tokenValidatedContext.Principal.Claims);
            claims.Add(new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId));
        }
                
        var result = await _userAccountService.GetUserAccounts(userId, email);

        var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
        var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
        claims.Add(associatedAccountsClaim);

        if (!_employerFinanceConfiguration.UseGovSignIn)
        {
            return claims;
        }

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