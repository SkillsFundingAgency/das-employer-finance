using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Infrastructure.OuterApiRequests;

[TestFixture]
internal class WhenBuildingGetLevyProjectionsByAccountIdRequest
{
    [Test, MoqAutoData]
    public void Then_The_Url_Is_Correct(long accountId, int months)
    {
        //Arrange
        var expectedUrl = $"finance/levy/{accountId}/projections?months={months}";
        //Act
        var actual = new GetLevyProjectionsByAccountIdRequest(accountId, months);
        //Assert
        actual.GetUrl.Should().Be(expectedUrl);
    }
}