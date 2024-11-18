using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Models.UserAccounts;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Authorization;
using SFA.DAS.EmployerFinance.Web.Extensions;

namespace SFA.DAS.EmployerFinance.Web.Authentication;

public interface IEmployerAccountAuthorisationHandler
{
    Task<bool> IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles);
    Task<bool> CheckUserAccountAccess(EmployerUserRole userRoleRequired);
}

public class EmployerAccountAuthorisationHandler(
    IHttpContextAccessor httpContextAccessor,
    IUserAccountService accountsService,
    ILogger<EmployerAccountOwnerAuthorizationHandler> logger)
    : IEmployerAccountAuthorisationHandler
{
    public async Task<bool> IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles)
    {
        var user = httpContextAccessor.HttpContext?.User;
        
        // If the user is redirected to a controller action from another site (very likely) and this is method is executed, the claims will be empty until the middleware has
        // re-authenticated the user. Once authentication is confirmed this method will be executed again with the claims populated and will run properly.
        if (user.ClaimsAreEmpty())
        {
            logger.LogInformation("CF: User claims are empty");
            return false;
        }
        
        if (!httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey(RouteValueKeys.HashedAccountId))
        {
            logger.LogInformation("CF: ID not in url");
            return false;
        }
        var accountIdFromUrl = httpContextAccessor.HttpContext.Request.RouteValues[RouteValueKeys.HashedAccountId].ToString().ToUpper();
        var employerAccountClaim = context.User.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        if (employerAccountClaim?.Value == null)
        {
            logger.LogInformation("CF: No employer account claim found");
            return false;
        }

        Dictionary<string, EmployerUserAccountItem> employerAccounts;

        try
        {
            employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value);
        }
        catch (JsonSerializationException e)
        {
            logger.LogError(e, "Could not deserialize employer account claim for user");
            return false;
        }

        EmployerUserAccountItem employerIdentifier = null;

        if (employerAccounts != null)
        {
            employerIdentifier = employerAccounts.TryGetValue(accountIdFromUrl, out var account)
                ? account : null;
        }

        if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
        {
            if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
                return false;

            var userClaim = context.User.Claims
                .First(c => c.Type.Equals(ClaimTypes.NameIdentifier));

            var email = context.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;

            var userId = userClaim.Value;

            var result = await accountsService.GetUserAccounts(userId, email);

            var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
            logger.LogInformation("CF: {AccountsAsJson}", accountsAsJson);
            var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

            var updatedEmployerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim.Value);

            userClaim.Subject.AddClaim(associatedAccountsClaim);

            if (!updatedEmployerAccounts.ContainsKey(accountIdFromUrl))
            {
                return false;
            }
            employerIdentifier = updatedEmployerAccounts[accountIdFromUrl];
        }

        if (!httpContextAccessor.HttpContext.Items.ContainsKey("Employer"))
        {
            httpContextAccessor.HttpContext.Items.Add("Employer", employerAccounts.GetValueOrDefault(accountIdFromUrl));
        }

        if (!CheckUserRoleForAccess(employerIdentifier, allowAllUserRoles))
        {
            return false;
        }

        return true;
    }

    public async Task<bool> CheckUserAccountAccess(EmployerUserRole userRoleRequired)
    {
        var context = httpContextAccessor.HttpContext;
        if (!context.Request.RouteValues.ContainsKey(RouteValueKeys.HashedAccountId))
        {
            return false;
        }

        Dictionary<string, EmployerUserAccountItem> employerAccounts;
        var accountIdFromUrl = context.Request.RouteValues[RouteValueKeys.HashedAccountId].ToString().ToUpper();
        var employerAccountClaim = context.User.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        logger.LogInformation("CF: {EmployerAccountClaim}", employerAccountClaim.Value);
        try
        {
            employerAccounts =
                JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim
                    .Value);
        }
        catch (JsonSerializationException e)
        {
            logger.LogError(e, "Could not deserialize employer account claim for user");
            return false;
        }
        
        if (!employerAccounts.Any())
        {
            logger.LogInformation("CF: no employer accounts");
            if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
                return false;

            var userClaim = context.User.Claims
                .First(c => c.Type.Equals(ClaimTypes.NameIdentifier));

            var email = context.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;

            var userId = userClaim.Value;

            var updatedEmployerAccounts = await accountsService.GetUserAccounts(userId, email);
            employerAccounts = updatedEmployerAccounts.EmployerAccounts.ToDictionary(x => x.AccountId);
        }

        if (employerAccounts == null || !employerAccounts.Any())
        {
            logger.LogInformation("Employer accounts null");
            return false;
        }

        logger.LogInformation("CF: Employer accounts: {EmployerAccounts}", JsonConvert.SerializeObject(employerAccounts));
        var employerIdentifier = employerAccounts.TryGetValue(accountIdFromUrl, out var account)
                ? account : null;

        if (employerIdentifier == null)
        {
            return false;
        }

        if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var claimUserRole))
        {
            return false;
        }

        logger.LogInformation("Claim user role: {ClaimUserRole}", claimUserRole.ToString());
        switch (userRoleRequired)
        {
            case EmployerUserRole.Owner when claimUserRole == EmployerUserRole.Owner:
            case EmployerUserRole.Transactor when claimUserRole is EmployerUserRole.Owner or EmployerUserRole.Transactor:
            case EmployerUserRole.Viewer when claimUserRole is EmployerUserRole.Owner or EmployerUserRole.Transactor or EmployerUserRole.Viewer:
                return true;
            default:
                return false;
        }
    }

    private static bool CheckUserRoleForAccess(EmployerUserAccountItem employerIdentifier, bool allowAllUserRoles)
    {
        if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole))
        {
            return false;
        }

        if (userRole == EmployerUserRole.None)
        {
            return false;
        }

        return allowAllUserRoles || userRole == EmployerUserRole.Owner;
    }
}