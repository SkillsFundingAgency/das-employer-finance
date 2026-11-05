using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountIdAndApprenticeshipId;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetPaymentsByAccountIdTests;

public class WhenIGetPaymentsByAccountId
{
    private const long AccountId = 12345;
    private const long ApprenticeshipId = 678;

    private Mock<IPaymentRepository> _paymentRepository;
    private GetPaymentsByAccountIdAndApprenticeshipIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _paymentRepository = new Mock<IPaymentRepository>();
        _handler = new GetPaymentsByAccountIdAndApprenticeshipIdQueryHandler(_paymentRepository.Object);
    }

    [Test]
    public async Task Then_All_Payments_For_The_Apprenticeship_Are_Returned()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = Guid.NewGuid(), EmployerAccountId = AccountId, ApprenticeshipId = ApprenticeshipId, Amount = 100 },
            new() { Id = Guid.NewGuid(), EmployerAccountId = AccountId, ApprenticeshipId = ApprenticeshipId, Amount = 150 }
        };
        _paymentRepository
            .Setup(r => r.GetPayments(AccountId, ApprenticeshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        // Act
        var result = await _handler.Handle(new GetPaymentsByAccountIdAndApprenticeshipIdQuery(AccountId, ApprenticeshipId), CancellationToken.None);

        // Assert
        result.Payments.Should().BeEquivalentTo(payments);
        _paymentRepository.Verify(r => r.GetPayments(AccountId, ApprenticeshipId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Then_An_Empty_Collection_Is_Returned_When_There_Are_No_Payments()
    {
        // Arrange
        _paymentRepository
            .Setup(r => r.GetPayments(AccountId, ApprenticeshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(new GetPaymentsByAccountIdAndApprenticeshipIdQuery(AccountId, ApprenticeshipId), CancellationToken.None);

        // Assert
        result.Payments.Should().BeEmpty();
    }
}