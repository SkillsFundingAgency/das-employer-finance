using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.UserAccounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.UserAccounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFinance.Services;

public class UserAccountService(IOuterApiClient outerApiClient) : IGovAuthEmployerAccountService
{
    public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
    {
        var actual = await outerApiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(email, userId));
        
        return new EmployerUserAccounts
        {
            EmployerAccounts = actual.UserAccounts != null? actual.UserAccounts.Select(c => new EmployerUserAccountItem
            {
                Role = c.Role,
                AccountId = c.AccountId,
                ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                EmployerName = c.EmployerName,
            }).ToList() : [],
            FirstName = actual.FirstName,
            IsSuspended = actual.IsSuspended,
            LastName = actual.LastName,
            EmployerUserId = actual.EmployerUserId,
        };
    }
}