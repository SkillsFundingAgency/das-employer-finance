using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Events.ProcessPayment;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;

public class RefreshPaymentDataCommandHandler : IRequestHandler<RefreshPaymentDataCommand, Unit>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<RefreshPaymentDataCommand> _validator;
    private readonly IPaymentService _paymentService;
    private readonly IDasLevyRepository _dasLevyRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<RefreshPaymentDataCommandHandler> _logger;


    public RefreshPaymentDataCommandHandler(
        IEventPublisher eventPublisher,
        IValidator<RefreshPaymentDataCommand> validator,
        IPaymentService paymentService,
        IDasLevyRepository dasLevyRepository,
        IMediator mediator,
        ILogger<RefreshPaymentDataCommandHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _validator = validator;
        _paymentService = paymentService;
        _dasLevyRepository = dasLevyRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Unit> Handle(RefreshPaymentDataCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        ICollection<PaymentDetails> payments = null;

        try
        {
            _logger.LogInformation($"GetAccountPayments for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

            payments = await _paymentService.GetAccountPayments(request.PeriodEnd, request.AccountId, request.CorrelationId);
        }
        catch (WebException ex)
        {
            _logger.LogError(ex, $"Unable to get payment information for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");
        }

        if (payments == null || !payments.Any())
        {
            _logger.LogInformation($"GetAccountPayments did not find any payments for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

            await PublishRefreshPaymentDataCompletedEvent(request, false);

            return Unit.Value;
        }

        _logger.LogInformation($"GetAccountPaymentIds for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

        var existingPaymentIds = await _dasLevyRepository.GetAccountPaymentIds(request.AccountId);
        var newPayments = payments.Where(p => !existingPaymentIds.Contains(p.Id)).ToArray();

        if (!newPayments.Any())
        {
            _logger.LogInformation($"No new payments for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}'");

            await PublishRefreshPaymentDataCompletedEvent(request, false);

            return Unit.Value;
        }

        _logger.LogInformation($"CreatePayments for new payments AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

        var newNonFullyFundedPayments = newPayments.Where(p => p.FundingSource != FundingSource.FullyFundedSfa);

        await _dasLevyRepository.CreatePayments(newNonFullyFundedPayments);
        await _mediator.Publish(new ProcessPaymentEvent { AccountId = request.AccountId }, cancellationToken);

        await PublishRefreshPaymentDataCompletedEvent(request, true);

        _logger.LogInformation($"Finished publishing ProcessPaymentEvent and PaymentCreatedMessage messages for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

        return Unit.Value;
    }

    private Task PublishRefreshPaymentDataCompletedEvent(RefreshPaymentDataCommand message, bool hasPayments)
    {
        return _eventPublisher.Publish(new RefreshPaymentDataCompletedEvent()
        {
            AccountId = message.AccountId,
            Created = DateTime.UtcNow,
            PeriodEnd = message.PeriodEnd,
            PaymentsProcessed = hasPayments
        });
    }
}