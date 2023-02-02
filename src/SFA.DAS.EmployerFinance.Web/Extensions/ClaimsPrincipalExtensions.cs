using System.Security.Claims;

namespace SFA.DAS.EmployerFinance.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return user.FindFirst(Em)
        }
    }
}
