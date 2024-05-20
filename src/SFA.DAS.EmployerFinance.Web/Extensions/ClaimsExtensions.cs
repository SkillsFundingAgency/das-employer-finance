using SFA.DAS.EmployerFinance.Infrastructure;

namespace SFA.DAS.EmployerFinance.Web.Extensions;

public static class ClaimsExtensions
{
    public static string GetValueFor(this ClaimsIdentity identity, string type)
    {
        return identity.Claims.FirstOrDefault(c => c.Type == type)?.Value;
    }
    
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
    
    public static bool ClaimsAreEmpty(this ClaimsPrincipal user)
    {
        return !user.Claims.Any();
    }
}