using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Queries.GetAccountPaymentIds;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountPaymentIdsTests;

public class WhenIGetAccountPaymentIds
{
    private Mock<IDasLevyRepository> _dasLevyRepositoryMock;
    private GetAccountPaymentIdsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _dasLevyRepositoryMock = new Mock<IDasLevyRepository>();
        _handler = new GetAccountPaymentIdsQueryHandler(_dasLevyRepositoryMock.Object);
    }

    [Test]
    public async Task Then_The_Repository_Is_Called_With_The_Correct_Parameters()
    {
        // Arrange
        var accountId = 12345L;

        var expectedIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };
        var expectedResponse = new GetAccountPaymentIdsResponse
        {
            PaymentIds = expectedIds
        };

        _dasLevyRepositoryMock
            .Setup(r => r.GetAccountPaymentIdsLinq(accountId, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedResponse);

        var request = new GetAccountPaymentIdsRequest
        {
            AccountId = accountId
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        _dasLevyRepositoryMock.VerifyAll();
        result.Should().NotBeNull();
        result.PaymentIds.Should().BeEquivalentTo(expectedIds);
    }

    [Test]
    public async Task Then_An_Empty_List_Is_Returned_If_No_PaymentIds_Found()
    {
        // Arrange
        _dasLevyRepositoryMock
            .Setup(r => r.GetAccountPaymentIdsLinq(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new GetAccountPaymentIdsResponse
            {
                PaymentIds = new List<Guid>()
            });

        var request = new GetAccountPaymentIdsRequest
        {
            AccountId = 98765L
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PaymentIds.Should().BeEmpty();
        _dasLevyRepositoryMock.VerifyAll();
    }


    [Test]
    public async Task Then_The_Response_Object_Is_Not_Null()
    {
        // Arrange
        _dasLevyRepositoryMock
            .Setup(r => r.GetAccountPaymentIdsLinq(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new GetAccountPaymentIdsResponse());

        var request = new GetAccountPaymentIdsRequest { AccountId = 111 };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
