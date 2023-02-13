using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.UserAccounts;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.UserAccounts;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.UserAccounts;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.UserAccountsTests;

public class WhenGettingUserAccounts
{
    [Test, MoqAutoData]
    public async Task Then_The_Outer_Api_Is_Called_And_Accounts_Returned(
        string email,
        string userId,
        GetUserAccountsResponse apiResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        UserAccountService service)
    {
        //Arrange
        var request = new GetUserAccountsRequest(email, userId);
        outerApiClient
            .Setup(x => x.Get<GetUserAccountsResponse>(
                It.Is<GetUserAccountsRequest>(c => c.GetUrl.Equals(request.GetUrl)))).ReturnsAsync(apiResponse);
        
        //Act
        var actual = await service.GetUserAccounts(userId, email);

        //Assert
        actual.ShouldBeEquivalentTo((EmployerUserAccounts)apiResponse);
    }
}