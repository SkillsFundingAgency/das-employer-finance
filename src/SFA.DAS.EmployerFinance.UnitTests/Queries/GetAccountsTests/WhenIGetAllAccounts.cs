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
        var expectedAccounts = new List<Account>
            {
                new Account { HashedId = "123", Name = "Account 1" },
                new Account { HashedId = "456", Name = "Account 2" }
            };

        _dasLevyRepositoryMock
            .Setup(r => r.GetAccounts(50, 2))
            .ReturnsAsync(expectedAccounts);

        var request = new GetAccountsRequest
        {
            PageSize = 50,
            PageNumber = 2
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        _dasLevyRepositoryMock.Verify(r => r.GetAccounts(50, 2), Times.Once);
        result.Should().NotBeNull();
        result.Accounts.Should().BeEquivalentTo(expectedAccounts);
    }

    [Test]
    public async Task Then_An_Empty_List_Is_Returned_If_No_Accounts_Found()
    {
        // Arrange
        _dasLevyRepositoryMock
            .Setup(r => r.GetAccounts(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Account>());

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
        _dasLevyRepositoryMock.Verify(r => r.GetAccounts(10, 1), Times.Once);
    }

    [Test]
    public async Task Then_The_Response_Object_Is_Not_Null()
    {
        // Arrange
        _dasLevyRepositoryMock
            .Setup(r => r.GetAccounts(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Account>());

        var request = new GetAccountsRequest { PageSize = 1, PageNumber = 1 };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
