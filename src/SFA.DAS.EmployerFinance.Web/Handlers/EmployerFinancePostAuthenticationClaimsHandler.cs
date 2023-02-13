using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerFinance.Configuration;

using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.Handlers
{
    public class EmployerFinancePostAuthenticationClaimsHandler : ICustomClaims
    {
        private readonly IConfiguration _configuration;
        private readonly EmployerFinanceConfiguration _employerFinanceConfiguration;
        private readonly IEmployer


        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            var claims = new List<Claim>();
            
        }
    }
}
