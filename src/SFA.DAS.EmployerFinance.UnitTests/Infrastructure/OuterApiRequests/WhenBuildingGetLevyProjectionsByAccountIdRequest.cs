using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Infrastructure.OuterApiRequests;

[TestFixture]
internal class WhenBuildingGetLevyProjectionsByAccountIdRequest
{
    [Test, MoqAutoData]
    public void Then_The_Url_Is_Correct(long accountId, int months, bool triggerRefresh)
    {
        //Arrange
        var expectedUrl = $"finance/levy/{accountId}/projections?months={months}&triggerRefresh={triggerRefresh}";
        //Act
        var actual = new GetLevyProjectionsByAccountIdRequest(accountId, months, triggerRefresh);
        //Assert
        actual.GetUrl.Should().Be(expectedUrl);
    }
}