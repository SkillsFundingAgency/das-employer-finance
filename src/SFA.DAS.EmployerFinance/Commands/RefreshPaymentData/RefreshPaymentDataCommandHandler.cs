using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Validation;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Events.ProcessPayment;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.NLog.Logger;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.Provider.Events.Api.Types;
using System.Threading;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentData
{
    public class RefreshPaymentDataCommandHandler : IRequestHandler<RefreshPaymentDataCommand,Unit>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IValidator<RefreshPaymentDataCommand> _validator;
        private readonly IPaymentService _paymentService;
        private readonly IDasLevyRepository _dasLevyRepository;
        private readonly IMediator _mediator;
        private readonly ILog _logger;


        public RefreshPaymentDataCommandHandler(
            IEventPublisher eventPublisher,
            IValidator<RefreshPaymentDataCommand> validator,
            IPaymentService paymentService,
            IDasLevyRepository dasLevyRepository,
            IMediator mediator,
            ILog logger)
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
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            ICollection<PaymentDetails> payments = null;

            try
            {
                _logger.Info($"GetAccountPayments for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

                payments = await _paymentService.GetAccountPayments(request.PeriodEnd, request.AccountId, request.CorrelationId);
            }
            catch (WebException ex)
            {
                _logger.Error(ex, $"Unable to get payment information for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");
            }

            if (payments == null || !payments.Any())
            {
                _logger.Info($"GetAccountPayments did not find any payments for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

                await PublishRefreshPaymentDataCompletedEvent(request, false);

                return Unit.Value;
            }

            _logger.Info($"GetAccountPaymentIds for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

            var failingPayment = payments.Where(p => p.ApprenticeshipId == 743445).ToList();

            var existingPaymentIds = await _dasLevyRepository.GetAccountPaymentIds(request.AccountId);
            var newPayments = payments.Where(p => !existingPaymentIds.Contains(p.Id)).ToArray();

            if (!newPayments.Any())
            {
                _logger.Info($"No new payments for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}'");

                await PublishRefreshPaymentDataCompletedEvent(request, false);

                return Unit.Value;
            }

            _logger.Info($"CreatePayments for new payments AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

            var newNonFullyFundedPayments = newPayments.Where(p => p.FundingSource != FundingSource.FullyFundedSfa);

            await _dasLevyRepository.CreatePayments(newNonFullyFundedPayments);
            await _mediator.PublishAsync(new ProcessPaymentEvent { AccountId = request.AccountId });

            await PublishRefreshPaymentDataCompletedEvent(request, true);

            _logger.Info($"Finished publishing ProcessPaymentEvent and PaymentCreatedMessage messages for AccountId = '{request.AccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

            return Unit.Value;
        }

        private async Task PublishRefreshPaymentDataCompletedEvent(RefreshPaymentDataCommand message, bool hasPayments)
        {
            await _eventPublisher.Publish(new RefreshPaymentDataCompletedEvent()
            {
                AccountId = message.AccountId,
                Created = DateTime.UtcNow,
                PeriodEnd = message.PeriodEnd,
                PaymentsProcessed = hasPayments
            });
        }
    }
}