using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.Handlers
{
    public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
    {
        private readonly EmployerFinanceWebConfiguration _employerFinanceConfiguration;
        private readonly IUserAccountService _userAccountService;
        private readonly IAuthenticationOrchestrator _authenticationOrchestrator;

        public EmployerAccountPostAuthenticationClaimsHandler(
            IUserAccountService userAccountService, 
            IOptions<EmployerFinanceWebConfiguration> employerFinanceConfiguration,
            IAuthenticationOrchestrator authenticationOrchestrator)
        {
            _userAccountService = userAccountService;
            _authenticationOrchestrator = authenticationOrchestrator;
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

                email = tokenValidatedContext.Principal.Claims
                .First(c => c.Type.Equals(EmployerClaims.IdamsUserEmailClaimTypeIdentifier)).Value;

                claims.AddRange(tokenValidatedContext.Principal.Claims);
                claims.Add(new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId));
            }
                
            var result = await _userAccountService.GetUserAccounts(userId, email);

            var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
            var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);
            claims.Add(associatedAccountsClaim);

            //TODO: This needs removing and was only added back into this area to facilitate the completion of the NET6 upgrade work. If finance is going to keep a local cache of users then it needs a better way to keep it in sync
            await _authenticationOrchestrator.SaveIdentityAttributes(userId, email, result.FirstName, result.LastName);

            if (!_employerFinanceConfiguration.UseGovSignIn)
            {
                return claims;
            }

            if (result.IsSuspended)
            {
                claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
            }

            claims.Add(new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, result.EmployerUserId));
            claims.Add(new Claim(EmployerClaims.IdamsUserDisplayNameClaimTypeIdentifier, result.FirstName + " " + result.LastName));

            return claims;
            
        }
    }
}
