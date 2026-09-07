using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Commands.ExpireAccountFunds;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.ExpiredFunds;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.ExpireAccountFundsTests;

public class WhenExpiringAccountFunds
{
    private static readonly DateTime Now = new(2026, 8, 12);

    private Mock<IValidator<ExpireAccountFundsCommand>> _validator = null!;
    private Mock<ICurrentDateTime> _currentDateTime = null!;
    private Mock<ILevyFundsInRepository> _levyFundsInRepository = null!;
    private Mock<IPaymentFundsOutRepository> _paymentFundsOutRepository = null!;
    private Mock<IExpiredFunds> _expiredFunds = null!;
    private Mock<IExpiredFundsRepository> _expiredFundsRepository = null!;
    private Mock<ILogger<ExpireAccountFundsCommandHandler>> _logger = null!;
    private EmployerFinanceConfiguration _configuration = null!;
    private ExpireAccountFundsCommandHandler _handler = null!;

    [SetUp]
    public void Arrange()
    {
        _validator = new Mock<IValidator<ExpireAccountFundsCommand>>();
        _validator
            .Setup(validator => validator.Validate(It.IsAny<ExpireAccountFundsCommand>()))
            .Returns(new ValidationResult());

        _currentDateTime = new Mock<ICurrentDateTime>();
        _currentDateTime.Setup(currentDateTime => currentDateTime.Now).Returns(Now);

        _levyFundsInRepository = new Mock<ILevyFundsInRepository>();
        _levyFundsInRepository
            .Setup(repository => repository.GetLevyFundsIn(It.IsAny<long>()))
            .ReturnsAsync([]);

        _paymentFundsOutRepository = new Mock<IPaymentFundsOutRepository>();
        _paymentFundsOutRepository
            .Setup(repository => repository.GetPaymentFundsOut(It.IsAny<long>()))
            .ReturnsAsync([]);

        _expiredFundsRepository = new Mock<IExpiredFundsRepository>();
        _expiredFundsRepository
            .Setup(repository => repository.Get(It.IsAny<long>()))
            .ReturnsAsync([]);

        _expiredFunds = new Mock<IExpiredFunds>();
        _expiredFunds
            .Setup(service => service.GetExpiringFunds(
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                It.IsAny<int>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()))
            .Returns(new Dictionary<CalendarPeriod, decimal>());

        _configuration = new EmployerFinanceConfiguration
        {
            FundsExpiryPeriod = 18,
            NewFundsExpiryPeriod = 12
        };
        _logger = new Mock<ILogger<ExpireAccountFundsCommandHandler>>();

        _handler = new ExpireAccountFundsCommandHandler(
            _validator.Object,
            _currentDateTime.Object,
            _levyFundsInRepository.Object,
            _paymentFundsOutRepository.Object,
            _expiredFunds.Object,
            _expiredFundsRepository.Object,
            _configuration,
            _logger.Object);
    }

    [Test]
    public void Then_Does_Not_Read_Or_Persist_When_Validation_Fails()
    {
        var command = CreateCommand();
        var validationResult = new ValidationResult();
        validationResult.AddError(nameof(command.AccountId), "AccountId must be greater than 0.");
        _validator.Setup(validator => validator.Validate(command)).Returns(validationResult);

        Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _levyFundsInRepository.Verify(
            repository => repository.GetLevyFundsIn(It.IsAny<long>()),
            Times.Never);
        _expiredFundsRepository.Verify(
            repository => repository.Create(
                It.IsAny<long>(),
                It.IsAny<IEnumerable<ExpiredFund>>(),
                It.IsAny<DateTime>(),
                It.IsAny<byte>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Then_Persists_LongTerm_Funds_And_The_Current_Period_Zero()
    {
        _expiredFunds
            .Setup(service => service.GetExpiringFunds(
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                _configuration.FundsExpiryPeriod,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()))
            .Returns(new Dictionary<CalendarPeriod, decimal>
            {
                [new CalendarPeriod(2026, 7)] = 50m
            });

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.AccountId.Should().Be(123);
        result.CorrelationId.Should().Be("corr-123");
        result.FundsExpired.Should().BeTrue();
        result.LongTermExpiredFundsCount.Should().Be(2);
        result.ShortTermExpiredFundsCount.Should().Be(0);
        _expiredFundsRepository.Verify(repository => repository.Create(
            123,
            It.Is<IEnumerable<ExpiredFund>>(funds =>
                funds.Count() == 2
                && funds.Any(fund =>
                    fund.CalendarPeriodYear == 2026
                    && fund.CalendarPeriodMonth == 7
                    && fund.Amount == -50m)
                && funds.Any(fund =>
                    fund.CalendarPeriodYear == 2026
                    && fund.CalendarPeriodMonth == 8
                    && fund.Amount == 0m)),
            Now,
            5,
            "corr-123"), Times.Once);
        _expiredFundsRepository.Verify(repository => repository.Create(
            It.IsAny<long>(),
            It.IsAny<IEnumerable<ExpiredFund>>(),
            It.IsAny<DateTime>(),
            6,
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Then_Persists_Both_Expiry_Types_After_The_Policy_Change()
    {
        _configuration.FundsExpiryPolicyChangeDate = new DateTime(2026, 6, 1);
        _expiredFunds.Reset();
        _expiredFunds
            .SetupSequence(service => service.GetExpiringFunds(
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                It.IsAny<IDictionary<CalendarPeriod, decimal>>(),
                It.IsAny<int>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()))
            .Returns(new Dictionary<CalendarPeriod, decimal>
            {
                [new CalendarPeriod(2026, 5)] = 40m
            })
            .Returns(new Dictionary<CalendarPeriod, decimal>
            {
                [new CalendarPeriod(2026, 7)] = 20m
            });

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.FundsExpired.Should().BeTrue();
        result.LongTermExpiredFundsCount.Should().Be(2);
        result.ShortTermExpiredFundsCount.Should().Be(2);
        _expiredFundsRepository.Verify(repository => repository.Create(
            123,
            It.IsAny<IEnumerable<ExpiredFund>>(),
            Now,
            5,
            "corr-123"), Times.Once);
        _expiredFundsRepository.Verify(repository => repository.Create(
            123,
            It.Is<IEnumerable<ExpiredFund>>(funds => funds.Any(fund => fund.Amount == -20m)),
            Now,
            6,
            "corr-123"), Times.Once);
    }

    [Test]
    public async Task Then_A_Replay_Does_Not_Reinsert_Existing_Current_Period_Zeros()
    {
        _configuration.FundsExpiryPolicyChangeDate = new DateTime(2026, 6, 1);
        _expiredFundsRepository
            .Setup(repository => repository.Get(123))
            .ReturnsAsync(
            [
                new ExpiredFund
                {
                    CalendarPeriodYear = 2026,
                    CalendarPeriodMonth = 8,
                    Amount = 0m,
                    TransactionType = 5,
                    CorrelationId = "corr-123"
                },
                new ExpiredFund
                {
                    CalendarPeriodYear = 2026,
                    CalendarPeriodMonth = 8,
                    Amount = 0m,
                    TransactionType = 6,
                    CorrelationId = "corr-123"
                }
            ]);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.FundsExpired.Should().BeFalse();
        result.LongTermExpiredFundsCount.Should().Be(0);
        result.ShortTermExpiredFundsCount.Should().Be(0);
        _expiredFundsRepository.Verify(repository => repository.Create(
            It.IsAny<long>(),
            It.IsAny<IEnumerable<ExpiredFund>>(),
            It.IsAny<DateTime>(),
            It.IsAny<byte>(),
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Then_A_Replay_Reports_FundsExpired_When_The_Same_Request_Previously_Persisted_Funds()
    {
        _expiredFundsRepository
            .Setup(repository => repository.Get(123))
            .ReturnsAsync(
            [
                new ExpiredFund
                {
                    CalendarPeriodYear = 2026,
                    CalendarPeriodMonth = 7,
                    Amount = -50m,
                    TransactionType = 5,
                    CorrelationId = "corr-123"
                },
                new ExpiredFund
                {
                    CalendarPeriodYear = 2026,
                    CalendarPeriodMonth = 8,
                    Amount = 0m,
                    TransactionType = 5,
                    CorrelationId = "corr-123"
                }
            ]);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.FundsExpired.Should().BeTrue();
        result.LongTermExpiredFundsCount.Should().Be(0);
        result.ShortTermExpiredFundsCount.Should().Be(0);
        _expiredFundsRepository.Verify(repository => repository.Create(
            It.IsAny<long>(),
            It.IsAny<IEnumerable<ExpiredFund>>(),
            It.IsAny<DateTime>(),
            It.IsAny<byte>(),
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Then_A_New_Request_Does_Not_Report_Funds_Expired_By_A_Previous_Request()
    {
        _expiredFundsRepository
            .Setup(repository => repository.Get(123))
            .ReturnsAsync(
            [
                new ExpiredFund
                {
                    CalendarPeriodYear = 2026,
                    CalendarPeriodMonth = 7,
                    Amount = -50m,
                    TransactionType = 5,
                    CorrelationId = "different-correlation-id"
                },
                new ExpiredFund
                {
                    CalendarPeriodYear = 2026,
                    CalendarPeriodMonth = 8,
                    Amount = 0m,
                    TransactionType = 5,
                    CorrelationId = "different-correlation-id"
                }
            ]);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.FundsExpired.Should().BeFalse();
        result.LongTermExpiredFundsCount.Should().Be(0);
        result.ShortTermExpiredFundsCount.Should().Be(0);
        _expiredFundsRepository.Verify(repository => repository.Create(
            It.IsAny<long>(),
            It.IsAny<IEnumerable<ExpiredFund>>(),
            It.IsAny<DateTime>(),
            It.IsAny<byte>(),
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Then_Logs_Context_And_Rethrows_Unexpected_Errors()
    {
        var expectedException = new InvalidOperationException("Finance database unavailable");
        _levyFundsInRepository
            .Setup(repository => repository.GetLevyFundsIn(123))
            .ThrowsAsync(expectedException);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(CreateCommand(), CancellationToken.None));

        exception.Should().BeSameAs(expectedException);
        _logger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() ==
                    "[CorrelationId: corr-123] Failed to expire funds for AccountId 123."),
                It.Is<Exception>(actual => ReferenceEquals(actual, expectedException)),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private static ExpireAccountFundsCommand CreateCommand()
    {
        return new ExpireAccountFundsCommand
        {
            AccountId = 123,
            CorrelationId = "corr-123"
        };
    }
}
