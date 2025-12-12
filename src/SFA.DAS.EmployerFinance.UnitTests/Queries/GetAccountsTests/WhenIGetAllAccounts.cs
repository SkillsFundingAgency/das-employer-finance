using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Queries.GetAccounts;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountsTests;

public class WhenIGetAllAccounts
{
    private Mock<IDasLevyRepository> _dasLevyRepositoryMock;
    private GetAccountsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _dasLevyRepositoryMock = new Mock<IDasLevyRepository>();
        _handler = new GetAccountsQueryHandler(_dasLevyRepositoryMock.Object);
    }

    [Test]
    public async Task Then_The_Repository_Is_Called_With_The_Correct_Parameters()
    {
        // Arrange
        var expectedAccounts = new List<Api.Types.Account>
    {
        new Api.Types.Account { Name = "Account 1" },
        new Api.Types.Account { Name = "Account 2" }
    };

        var expectedResponse = new GetAccountsResponse
        {
            Accounts = expectedAccounts,
            TotalCount = 2,
            TotalPages = 1,
            PageNumber = 2,
            PageSize = 50
        };

        _dasLevyRepositoryMock
            .Setup(r => r.GetAccounts(50, 2))
            .ReturnsAsync(expectedResponse);

        var request = new GetAccountsRequest
        {
            PageSize = 50,
            PageNumber = 2
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        _dasLevyRepositoryMock.VerifyAll();
        result.Should().NotBeNull();
        result.Accounts.Should().BeEquivalentTo(expectedAccounts);
        result.TotalPages.Should().Be(1);
        result.TotalCount.Should().Be(2);
    }


    [Test]
    public async Task Then_An_Empty_List_Is_Returned_If_No_Accounts_Found()
    {
        // Arrange
        var expectedResponse = new GetAccountsResponse
        {
            Accounts = new List<Api.Types.Account>(),
            TotalCount = 0,
            TotalPages = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _dasLevyRepositoryMock
            .Setup(r => r.GetAccounts(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedResponse);

        var request = new GetAccountsRequest
        {
            PageSize = 10,
            PageNumber = 1
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Accounts.Should().BeEmpty();
        _dasLevyRepositoryMock.VerifyAll();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);

        _dasLevyRepositoryMock.Verify(r => r.GetAccounts(10, 1), Times.Once);
    }


    [Test]
    public async Task Then_The_Response_Object_Is_Not_Null()
    {
        // Arrange
        var expectedResponse = new GetAccountsResponse
        {
            Accounts = new List<Api.Types.Account>(),
            TotalCount = 0,
            TotalPages = 0,
            PageNumber = 1,
            PageSize = 1
        };

        _dasLevyRepositoryMock
            .Setup(r => r.GetAccounts(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedResponse);

        var request = new GetAccountsRequest
        {
            PageSize = 1,
            PageNumber = 1
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

}
