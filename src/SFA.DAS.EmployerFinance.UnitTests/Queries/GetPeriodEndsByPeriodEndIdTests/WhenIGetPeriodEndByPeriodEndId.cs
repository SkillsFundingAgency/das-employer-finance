using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetPeriodEndsByPeriodEndIdTests;

[TestFixture]
public class WhenIGetPeriodEndByPeriodEndId
{
    private Mock<IDasLevyRepository> _dasLevyRepositoryMock;
    private GetPeriodEndByPeriodEndIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _dasLevyRepositoryMock = new Mock<IDasLevyRepository>();
        _handler = new GetPeriodEndByPeriodEndIdQueryHandler(_dasLevyRepositoryMock.Object);
    }

    [Test]
    public async Task Then_The_Repository_Is_Called_With_The_Correct_Id()
    {
        // Arrange
        var expectedPeriodEnd = new PeriodEnd { PeriodEndId = "PE-001" };

        _dasLevyRepositoryMock
            .Setup(r => r.GetPeriodEndById("PE-001"))
            .ReturnsAsync(expectedPeriodEnd);

        var request = new GetPeriodEndByPeriodEndIdRequest { PeriodEndId = "PE-001" };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        _dasLevyRepositoryMock.Verify(r => r.GetPeriodEndById("PE-001"), Times.Once);
        result.Should().NotBeNull();
        result.PeriodEnd.Should().BeEquivalentTo(expectedPeriodEnd);
    }

    [Test]
    public async Task Then_Null_Is_Returned_If_The_PeriodEnd_Is_Not_Found()
    {
        // Arrange
        _dasLevyRepositoryMock
            .Setup(r => r.GetPeriodEndById("PE-999"))
            .ReturnsAsync((PeriodEnd)null);

        var request = new GetPeriodEndByPeriodEndIdRequest { PeriodEndId = "PE-999" };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PeriodEnd.Should().BeNull();
    }
}