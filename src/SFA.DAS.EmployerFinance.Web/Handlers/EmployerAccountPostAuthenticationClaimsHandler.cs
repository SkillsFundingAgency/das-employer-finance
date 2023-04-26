using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.Handlers
{
    public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
    {
        private readonly EmployerFinanceWebConfiguration _employerFinanceConfiguration;
        private readonly IUserAccountService _userAccountService;

        public EmployerAccountPostAuthenticationClaimsHandler(IUserAccountService userAccountService, IOptions<EmployerFinanceWebConfiguration> employerFinanceConfiguration)
        {
            _userAccountService = userAccountService;
            _employerFinanceConfiguration = employerFinanceConfiguration.Value;
        }

        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            var claims = new List<Claim>();
        
        
            string userId;
            var email = string.Empty;
            
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
            }
                
            var result = await _userAccountService.GetUserAccounts(userId, email);
            
            if (result.IsSuspended)
            {
                claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
            }

            var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
            var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
            claims.Add(associatedAccountsClaim);
            if (!_employerFinanceConfiguration.UseGovSignIn)
            {
                return claims;
            }
                
            claims.Add(new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, result.EmployerUserId));
            claims.Add(new Claim(EmployerClaims.IdamsUserDisplayNameClaimTypeIdentifier, result.FirstName + " " + result.LastName));
            
            return claims;
            
        }
    }
}
