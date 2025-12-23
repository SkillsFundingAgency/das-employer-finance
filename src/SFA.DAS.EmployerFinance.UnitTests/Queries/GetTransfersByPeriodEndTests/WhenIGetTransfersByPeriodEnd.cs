using Moq;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetTransfersByPeriodEndTests;

[TestFixture]
public class WhenIGetTransfersByPeriodEnd
{
    private Mock<IDasLevyRepository> _dasLevyRepositoryMock;
    private GetTransfersByPeriodEndQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _dasLevyRepositoryMock = new Mock<IDasLevyRepository>();
        _handler = new GetTransfersByPeriodEndQueryHandler(_dasLevyRepositoryMock.Object);
    }

    [Test]
    public async Task Then_The_Repository_Is_Called_With_The_Correct_Parameters()
    {
        // Arrange
        long expectedAccountId = 12345;
        string expectedPeriodEnd = "2023-R01";

        var expectedTransfers = new List<AccountTransfer>
            {
                new AccountTransfer { SenderAccountId = expectedAccountId, PeriodEnd = expectedPeriodEnd }
            };

        _dasLevyRepositoryMock
            .Setup(r => r.GetTransfersByPeriodEnd(expectedAccountId, expectedPeriodEnd))
            .ReturnsAsync(expectedTransfers);

        var request = new GetTransfersByPeriodEndRequest
        {
            AccountId = expectedAccountId,
            PeriodEnd = expectedPeriodEnd
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        _dasLevyRepositoryMock.Verify(
            r => r.GetTransfersByPeriodEnd(expectedAccountId, expectedPeriodEnd),
            Times.Once);

        result.Should().NotBeNull();
        result.AccountTransfers.Should().BeEquivalentTo(expectedTransfers);
    }

    [Test]
    public async Task Then_An_Empty_List_Is_Returned_If_No_Transfers_Are_Found()
    {
        // Arrange
        long expectedAccountId = 99999;
        string expectedPeriodEnd = "2099-R99";

        _dasLevyRepositoryMock
            .Setup(r => r.GetTransfersByPeriodEnd(expectedAccountId, expectedPeriodEnd))
            .ReturnsAsync(new List<AccountTransfer>());

        var request = new GetTransfersByPeriodEndRequest
        {
            AccountId = expectedAccountId,
            PeriodEnd = expectedPeriodEnd
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccountTransfers.Should().NotBeNull();
        result.AccountTransfers.Should().BeEmpty();
    }
}