﻿using IdentityModel.Client;
using MediatR;
using SFA.DAS.Authentication;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerUsers.WebClientComponents;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace SFA.DAS.EmployerAccounts.Web.Authentication
{
    public class OwinAuthenticationService : IAuthenticationService
    {
        private readonly EmployerAccountsConfiguration _configuration;
        private readonly HttpContextBase _httpContext;

        public OwinAuthenticationService(EmployerAccountsConfiguration configuration, HttpContextBase httpContext)
        {
            _configuration = configuration;
            _httpContext = httpContext;
        }

        public string GetClaimValue(string key)
        {
            var claimIdentity = ((ClaimsIdentity)_httpContext.User.Identity).Claims.FirstOrDefault(c => c.Type == key);
            return claimIdentity == null ? "" : claimIdentity.Value;
        }

        public bool HasClaim(string type, string value)
        {
            return ((ClaimsIdentity)_httpContext.User.Identity).HasClaim(type, value);
        }

        public bool IsUserAuthenticated()
        {
            return _httpContext.GetOwinContext().Authentication.User.Identity.IsAuthenticated;
        }

        public void SignOutUser()
        {
            var owinContext = HttpContext.Current.GetOwinContext();
            var authenticationManager = owinContext.Authentication;

            authenticationManager.SignOut("Cookies");
        }

        public bool TryGetClaimValue(string key, out string value)
        {
            var identity = _httpContext.User.Identity as ClaimsIdentity;
            var claim = identity?.Claims.FirstOrDefault(c => c.Type == key);

            value = claim?.Value;

            return value != null;
        }

        public async Task UpdateClaims()
        {
            var constants = new Constants(_configuration.Identity);
            var userInfoEndpoint = constants.UserInfoEndpoint();
            var accessToken = GetClaimValue("access_token");
            var userInfoClient = new UserInfoClient(new Uri(userInfoEndpoint), accessToken);
            var userInfo = await userInfoClient.GetAsync();
            var identity = (ClaimsIdentity)HttpContext.Current.User.Identity;

            foreach (var claim in userInfo.Claims.ToList())
            {
                if (claim.Item1.Equals(DasClaimTypes.Email))
                {
                    var emailClaim = identity.Claims.FirstOrDefault(c => c.Type == "email");
                    var emailClaim2 = identity.Claims.FirstOrDefault(c => c.Type == DasClaimTypes.Email);

                    identity.RemoveClaim(emailClaim);
                    identity.RemoveClaim(emailClaim2);
                    identity.AddClaim(new Claim("email", claim.Item2));
                    identity.AddClaim(new Claim(DasClaimTypes.Email, claim.Item2));
                }

                if (claim.Item1.Equals(DasClaimTypes.RequiresVerification))
                {
                    var requiresValidationClaim =
                        identity.Claims.FirstOrDefault(c => c.Type == DasClaimTypes.RequiresVerification);

                    if (requiresValidationClaim != null)
                    {
                        identity.RemoveClaim(requiresValidationClaim);
                    }
                    identity.AddClaim(new Claim(DasClaimTypes.RequiresVerification, claim.Item2));
                }                
            }
        }
    }
}