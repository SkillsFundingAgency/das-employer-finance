using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Queries.GetAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountsByIdTests;

[TestFixture]
public class WhenIGetAccountById
{
    private Mock<IDasLevyRepository> _dasLevyRepositoryMock;
    private GetAccountByIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _dasLevyRepositoryMock = new Mock<IDasLevyRepository>();
        _handler = new GetAccountByIdQueryHandler(_dasLevyRepositoryMock.Object);
    }

    [Test]
    public async Task Then_The_Repository_Is_Called_With_The_Correct_Id()
    {
        // Arrange
        var expectedAccount = new Account { HashedId = "123", Name = "Test Account" };

        _dasLevyRepositoryMock
            .Setup(r => r.GetAccountById(123))
            .ReturnsAsync(expectedAccount);

        var request = new GetAccountByIdRequest { AccountId = 123 };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        _dasLevyRepositoryMock.Verify(r => r.GetAccountById(123), Times.Once);
        result.Should().NotBeNull();
        result.Account.Should().BeEquivalentTo(expectedAccount);
    }

    [Test]
    public async Task Then_Null_Is_Returned_If_The_Account_Is_Not_Found()
    {
        // Arrange
        _dasLevyRepositoryMock
            .Setup(r => r.GetAccountById(999))
            .ReturnsAsync((Account)null);

        var request = new GetAccountByIdRequest { AccountId = 999 };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Account.Should().BeNull();
        _dasLevyRepositoryMock.Verify(r => r.GetAccountById(999), Times.Once);
    }

    [Test]
    public async Task Then_The_Response_Object_Is_Not_Null()
    {
        // Arrange
        _dasLevyRepositoryMock
            .Setup(r => r.GetAccountById(It.IsAny<long>()))
            .ReturnsAsync((Account)null);

        var request = new GetAccountByIdRequest { AccountId = 1 };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
