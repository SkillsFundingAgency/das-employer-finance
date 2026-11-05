using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetPaymentFundsOutByAccountId;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetPaymentFundsOutByAccountIdTests;

[TestFixture]
public class WhenIGetPaymentFundsOutByAccountId
{
    private const long AccountId = 12345;

    private Mock<IPaymentFundsOutRepository> _paymentFundsOutRepository;
    private GetPaymentFundsOutByAccountIdQueryHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _paymentFundsOutRepository = new Mock<IPaymentFundsOutRepository>();
        _handler = new GetPaymentFundsOutByAccountIdQueryHandler(_paymentFundsOutRepository.Object);
    }

    [Test]
    public async Task Then_Funds_Out_Are_Returned_By_Calendar_Period_As_Positive_Amounts()
    {
        _paymentFundsOutRepository.Setup(x => x.GetPaymentFundsOut(AccountId)).ReturnsAsync(new List<PaymentFundsOut>
        {
            new() { CalendarPeriodYear = 2026, CalendarPeriodMonth = 4, FundsOut = -300 }
        });

        var result = await _handler.Handle(new GetPaymentFundsOutByAccountIdQuery(AccountId), CancellationToken.None);

        result.FundsOut.Should().BeEquivalentTo(new Dictionary<CalendarPeriod, decimal>
        {
            { new CalendarPeriod(2026, 4), 300 }
        });
    }

    [Test]
    public async Task Then_An_Empty_Dictionary_Is_Returned_When_There_Are_No_Funds_Out()
    {
        _paymentFundsOutRepository.Setup(x => x.GetPaymentFundsOut(AccountId)).ReturnsAsync(new List<PaymentFundsOut>());

        var result = await _handler.Handle(new GetPaymentFundsOutByAccountIdQuery(AccountId), CancellationToken.None);

        result.FundsOut.Should().NotBeNull().And.BeEmpty();
    }
}
