using System.Security.Claims;
using SFA.DAS.EmployerFinance.Infrastructure;

namespace SFA.DAS.EmployerFinance.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return user.FindFirst(EmployerClaims.IdamsUserIdClaimTypeIdentifier)?.Value;
        }

        public static string GetEmailAddress(this ClaimsPrincipal user)
        {
            return user.FindFirst(EmployerClaims.IdamsUserIdClaimTypeIdentifier)?.Value;
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(EmployerClaims.IdamsUserIdClaimTypeIdentifier)?.Value;
        }
    }
}
