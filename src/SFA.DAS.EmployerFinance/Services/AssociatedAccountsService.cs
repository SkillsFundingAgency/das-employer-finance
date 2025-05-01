using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFinance.Services;

public interface IAssociatedAccountsService
{
    Task<Dictionary<string, EmployerUserAccountItem>> GetAccounts(bool forceRefresh);
}

public class AssociatedAccountsService : IAssociatedAccountsService
{
    private readonly IGovAuthEmployerAccountService _accountsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AssociatedAccountsService> _logger;

    // To allow unit testing
    public int MaxPermittedNumberOfAccountsOnClaim { get; set; } = Constants.MaxNumberOfAssociatedAccountsAllowedOnClaim;

    public AssociatedAccountsService(
        IGovAuthEmployerAccountService accountsService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AssociatedAccountsService> logger)
    {
        _accountsService = accountsService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a users associated employer accounts from claims.
    /// If the claim is null, the data will be pulled from UserAccountService and persisted to the claims for caching purposes.
    /// </summary>
    /// <param name="forceRefresh">Forces data to be refreshed from UserAccountsService and persisted to user claims regardless of claims state.</param>
    /// <returns>Dictionary of string, EmployerUserAccountItem</returns>
    public async Task<Dictionary<string, EmployerUserAccountItem>> GetAccounts(bool forceRefresh)
    {
        var user = _httpContextAccessor.HttpContext.User;
        
        if (ClaimsAreEmpty(user))
        {
            return null;
        }
        
        var employerAccountsClaim = user.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        if (!forceRefresh && employerAccountsClaim != null)
        {
            try
            {
                var accountsFromClaim = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountsClaim.Value);

                // Some users have 100's of employer accounts. The claims cannot handle that volume of data,
                // so the claim may have been added for authorization purposes, but the claim itself is empty.
                if (accountsFromClaim != null && accountsFromClaim.Count > 0)
                {
                    return accountsFromClaim;
                }
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e, "Could not deserialize employer account claim for user");
                throw;
            }
        }

        var userClaim = user.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier));
        var email = user.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
        var userId = userClaim.Value;

        var result = await _accountsService.GetUserAccounts(userId, email);
        var associatedAccounts = result.EmployerAccounts.ToDictionary(k => k.AccountId);

        if (forceRefresh)
        {
            PersistToClaims(associatedAccounts, employerAccountsClaim, userClaim);
        }

        return associatedAccounts;
    }
    
    private void PersistToClaims(Dictionary<string, EmployerUserAccountItem> associatedAccounts, Claim employerAccountsClaim, Claim userClaim)
    {
        // Some users have 100's of employer accounts. The claims cannot handle that volume of data.
        var accountsAsJson = JsonConvert.SerializeObject(associatedAccounts.Count <= MaxPermittedNumberOfAccountsOnClaim
            ? associatedAccounts
            : new Dictionary<string, EmployerUserAccountItem>());

        var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

        if (employerAccountsClaim != null)
        {
            userClaim.Subject!.RemoveClaim(employerAccountsClaim);
        }

        userClaim.Subject!.AddClaim(associatedAccountsClaim);
    }

    private bool ClaimsAreEmpty(ClaimsPrincipal user)
    {
        return user == null || !user.Claims.Any() || !user.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier));
    }
}

public static class Constants
{
    public const int MaxNumberOfAssociatedAccountsAllowedOnClaim = 100;
}

public static class EmployerClaims
{
    public const string AccountsClaimsTypeIdentifier = "employer_accounts";
} 