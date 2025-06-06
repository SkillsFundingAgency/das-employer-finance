﻿using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.NServiceBus.Testing.Services;
using SFA.DAS.Testing;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshPaymentDataTests;

[TestFixture]
public class RefreshPaymentDataCommandHandlerTests : FluentTest<RefreshPaymentDataCommandHandlerTestsFixture>
{
    [Test]
    public Task WhenIHaveCompletedProcessing_AndHaveNewPayments()
    {
        const long accountId = 999;

        return TestAsync(f => f
                .SetAccountId(accountId)
                .SetPeriodEnd("2017R14")
                .SetExistingPayments(new List<Guid>
                {
                    Guid.Parse("953EC305-06FD-483C-A155-50211921143C"),
                    Guid.Parse("963EC305-06FD-483C-A155-50211921143C"),
                    Guid.Parse("973EC305-06FD-483C-A155-50211921143C"),
                })
                .SetIncomingPayments(new List<PaymentDetails>
                {
                    new PaymentDetails
                    {
                        Id = Guid.NewGuid(),
                        Amount = 99,
                        EmployerAccountId = accountId,
                    }

                })
            , f => f.Handle(), (f, r) =>
            {
                f.VerifyRefreshPaymentDataCompletedEventIsPublished(true);
            });
    }

    [Test]
    public Task WhenIHaveCompletedProcessing_AndHaveReceivedOnlyMatchingPayments()
    {
        const long accountId = 999;
        var paymentGuid = Guid.Parse("953EC305-06FD-483C-A155-50211921143C");

        return TestAsync(f => f
                .SetAccountId(accountId)
                .SetPeriodEnd("2017R14")
                .SetExistingPayments(new List<Guid>
                {
                    paymentGuid,
                    Guid.Parse("963EC305-06FD-483C-A155-50211921143C"),
                    Guid.Parse("973EC305-06FD-483C-A155-50211921143C"),
                })
                .SetIncomingPayments(new List<PaymentDetails>
                {
                    new PaymentDetails
                    {
                        Id = paymentGuid,
                        Amount = 99,
                        EmployerAccountId = accountId,
                    }

                })
            , f => f.Handle(), (f, r) =>
            {
                f.VerifyRefreshPaymentDataCompletedEventIsPublished(false);
            });
    }

    [Test]
    public Task WhenIHaveCompletedProcessing_AndHaveReceievedNoPayments()
    {
        const long accountId = 999;

        return TestAsync(f => f
                .SetAccountId(accountId)
                .SetPeriodEnd("2017R14")
                .SetExistingPayments(new List<Guid>
                {
                    Guid.Parse("963EC305-06FD-483C-A155-50211921143C"),
                    Guid.Parse("973EC305-06FD-483C-A155-50211921143C"),
                })
                .SetIncomingPayments(new List<PaymentDetails>())
            , f => f.Handle(), (f, r) =>
            {
                f.VerifyRefreshPaymentDataCompletedEventIsPublished(false);
            });
    }


    [Test]
    public Task WhenIHaveCompletedProcessing_AndHaveReceievedNullPayments()
    {
        const long accountId = 999;

        return TestAsync(f => f
                .SetAccountId(accountId)
                .SetPeriodEnd("2017R14")
                .SetExistingPayments(new List<Guid>
                {
                    Guid.Parse("963EC305-06FD-483C-A155-50211921143C"),
                    Guid.Parse("973EC305-06FD-483C-A155-50211921143C"),
                })
                .SetIncomingPayments(null)
            , f => f.Handle(), (f, r) =>
            {
                f.VerifyRefreshPaymentDataCompletedEventIsPublished(false);
            });
    }
}

public class RefreshPaymentDataCommandHandlerTestsFixture 
{
    private readonly Mock<IDasLevyRepository> _dasLevyRepository;
    private readonly Mock<IPaymentService> _paymentService;
    private readonly RefreshPaymentDataCommandHandler _handler;
    private readonly TestableEventPublisher eventPublisher;

    private long _accountId = 999;
    private string _periodEnd = "2018R12";

    public RefreshPaymentDataCommandHandlerTestsFixture()
    {
        _dasLevyRepository = new Mock<IDasLevyRepository>();
        var logger = new Mock<ILogger<RefreshPaymentDataCommandHandler>>();
        var mediator = new Mock<IMediator>();
        _paymentService = new Mock<IPaymentService>();
        var validator = new Mock<IValidator<RefreshPaymentDataCommand>>();
        eventPublisher = new TestableEventPublisher();

        validator.Setup(x => x.Validate(It.IsAny<RefreshPaymentDataCommand>()))
            .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });


        _handler = new RefreshPaymentDataCommandHandler(
            eventPublisher,
            validator.Object,
            _paymentService.Object,
            _dasLevyRepository.Object,
            mediator.Object,
            logger.Object);
    }

    public RefreshPaymentDataCommandHandlerTestsFixture SetAccountId(long accountId)
    {
        _accountId = accountId;

        return this;
    }
        
    public RefreshPaymentDataCommandHandlerTestsFixture SetPeriodEnd(string periodEnd)
    {
        _periodEnd = periodEnd;

        return this;
    }

    public RefreshPaymentDataCommandHandlerTestsFixture SetExistingPayments(List<Guid> guids)
    {
        _dasLevyRepository.Setup(x => x.GetAccountPaymentIds(It.IsAny<long>()))
            .ReturnsAsync(new HashSet<Guid>(guids));

        return this;
    }

    public RefreshPaymentDataCommandHandlerTestsFixture SetIncomingPayments(List<PaymentDetails> paymentDetails)
    {
        _paymentService.Setup(x => x.GetAccountPayments(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Guid>()))
            .ReturnsAsync(paymentDetails);

        return this;
    }

    public Task<RefreshPaymentDataResponse> Handle()
    {
        return _handler.Handle(new RefreshPaymentDataCommand
        {
            AccountId = _accountId,
            PeriodEnd = _periodEnd
        }, CancellationToken.None);
    }

    public void VerifyRefreshPaymentDataCompletedEventIsPublished(bool expectedPaymentProcessedValue)
    {
        (eventPublisher.Events.OfType<RefreshPaymentDataCompletedEvent>().Any(e =>
            e.AccountId == _accountId
            && e.PeriodEnd == _periodEnd
            && e.PaymentsProcessed == expectedPaymentProcessedValue)).Should().BeTrue();
    }
}