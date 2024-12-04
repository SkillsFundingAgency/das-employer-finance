using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Authorization;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFinance.Web.Authentication;

public interface IEmployerAccountAuthorisationHandler
{
    Task<bool> IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles);
    Task<bool> CheckUserAccountAccess(EmployerUserRole userRoleRequired);
}

public class EmployerAccountAuthorisationHandler(
    IHttpContextAccessor httpContextAccessor,
    IAssociatedAccountsService associatedAccountsService,
    ILogger<EmployerAccountOwnerAuthorizationHandler> logger)
    : IEmployerAccountAuthorisationHandler
{
    public async Task<bool> IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles)
    {
        var user = httpContextAccessor.HttpContext?.User;

        // If the user is redirected to a controller action from another site (very likely) and this is method is executed, the claims will be empty until the middleware
        // has re-authenticated the user. Once authentication is confirmed this method will be executed again with the claims populated and will run properly.
        if (user.ClaimsAreEmpty())
        {
            return false;
        }

        if (!httpContextAccessor.HttpContext!.Request.RouteValues.ContainsKey(RouteValueKeys.HashedAccountId))
        {
            return false;
        }

        var accountIdFromUrl = httpContextAccessor.HttpContext.Request.RouteValues[RouteValueKeys.HashedAccountId].ToString().ToUpper();

        Dictionary<string, EmployerUserAccountItem> employerAccounts;

        try
        {
            employerAccounts = await associatedAccountsService.GetAccounts(forceRefresh: false);
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
                ? account
                : null;
        }

        if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
        {
            if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
            {
                return false;
            }

            var updatedEmployerAccounts = await associatedAccountsService.GetAccounts(forceRefresh: true);

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

        return CheckUserRoleForAccess(employerIdentifier, allowAllUserRoles);
    }

    public async Task<bool> CheckUserAccountAccess(EmployerUserRole userRoleRequired)
    {
        var context = httpContextAccessor.HttpContext;
        if (!context.Request.RouteValues.ContainsKey(RouteValueKeys.HashedAccountId))
        {
            return false;
        }

        var accountIdFromUrl = context.Request.RouteValues[RouteValueKeys.HashedAccountId].ToString().ToUpper();

        Dictionary<string, EmployerUserAccountItem> employerAccounts;

        try
        {
            employerAccounts = await associatedAccountsService.GetAccounts(forceRefresh: false);
        }
        catch (JsonSerializationException e)
        {
            logger.LogError(e, "Could not retrieve employer accounts for user");
            return false;
        }

        if (employerAccounts.Count == 0)
        {
            if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
            {
                return false;
            }

            employerAccounts = await associatedAccountsService.GetAccounts(forceRefresh: true);
        }

        if (employerAccounts == null || employerAccounts.Count == 0)
        {
            logger.LogInformation("Employer accounts null");
            return false;
        }

        var employerIdentifier = employerAccounts.TryGetValue(accountIdFromUrl, out var account)
            ? account
            : null;

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