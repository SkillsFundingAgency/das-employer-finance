using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetPeriodEndsByPeriodEndIdTests;

[TestFixture]
public class WhenIGetPeriodEndByPeriodEndId
{
    private Mock<IDasLevyRepository> _dasLevyRepositoryMock;
    private GetPeriodEndQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _dasLevyRepositoryMock = new Mock<IDasLevyRepository>();
        _handler = new GetPeriodEndQueryHandler(_dasLevyRepositoryMock.Object);
    }

    [Test]
    public async Task Then_The_Repository_Is_Called_To_Get_All_PeriodEnds()
    {
        // Arrange
        var periodEnds = new List<PeriodEnd>
        {
            new() { PeriodEndId = "PE-001" },
            new() { PeriodEndId = "PE-002" }
        };

        _dasLevyRepositoryMock
            .Setup(r => r.GetAllPeriodEnds())
            .Returns(Task.FromResult<IEnumerable<PeriodEnd>>(periodEnds));

        var request = new GetPeriodEndsRequest();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        _dasLevyRepositoryMock.Verify(r => r.GetAllPeriodEnds(), Times.Once);
        result.Should().NotBeNull();
        result.CurrentPeriodEnds.Should().BeEquivalentTo(periodEnds);
    }

    [Test]
    public async Task Then_An_Empty_List_Is_Returned_If_No_PeriodEnds_Found()
    {
        // Arrange
        _dasLevyRepositoryMock
            .Setup(r => r.GetAllPeriodEnds())
            .ReturnsAsync(new List<PeriodEnd>());

        var request = new GetPeriodEndsRequest();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CurrentPeriodEnds.Should().NotBeNull();
        result.CurrentPeriodEnds.Should().BeEmpty();
    }
}