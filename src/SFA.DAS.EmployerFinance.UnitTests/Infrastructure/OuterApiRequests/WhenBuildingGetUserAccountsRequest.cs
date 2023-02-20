using System.Net;
using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.UserAccounts;

namespace SFA.DAS.EmployerFinance.UnitTests.Infrastructure.OuterApiRequests;

public class WhenBuildingGetUserAccountsRequest
{
    [Test, AutoData]
    public void Then_The_Url_Is_Correctly_Formatted_With_Encoded_Email(string email, string userId)
    {
        email = email + "'test @+£@$" + email; 
        
        var actual = new GetUserAccountsRequest(email, userId);

        actual.GetUrl.Should().Be($"accountusers/{userId}/accounts?email={WebUtility.UrlEncode(email)}");
    }
}