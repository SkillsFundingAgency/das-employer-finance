using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Infrastructure.OuterApiRequests;

[TestFixture]
internal class WhenBuildingGetLevySummaryByAccountIdRequest
{
    [Test, MoqAutoData]
    public void Then_The_Url_Is_Correct(long accountId)
    {
        //Arrange
        var expectedUrl = $"finance/levy/{accountId}/summary";
        //Act
        var actual = new GetLevySummaryByAccountIdRequest(accountId);
        //Assert
        actual.GetUrl.Should().Be(expectedUrl);
    }
}