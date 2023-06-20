using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.UserAccounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.UserAccounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.UserAccounts;

namespace SFA.DAS.EmployerFinance.Services;

public interface IUserAccountService
{
    Task<EmployerUserAccounts> GetUserAccounts(string userId, string email);
}

public class UserAccountService : IUserAccountService
{
    private readonly IOuterApiClient _outerApiClient;

    public UserAccountService(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }
    public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
    {
        var actual = await _outerApiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(email, userId));

        return actual;
    }
}