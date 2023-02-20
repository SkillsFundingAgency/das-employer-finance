using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;

public class RefreshAccountTransfersCommandHandler : IRequestHandler<RefreshAccountTransfersCommand, Unit>
{
    private readonly IValidator<RefreshAccountTransfersCommand> _validator;
    private readonly IPaymentService _paymentService;
    private readonly ITransferRepository _transferRepository;
    private readonly ILogger<RefreshAccountTransfersCommandHandler> _logger;

    public RefreshAccountTransfersCommandHandler(
        IValidator<RefreshAccountTransfersCommand> validator,
        IPaymentService paymentService,
        ITransferRepository transferRepository,
        ILogger<RefreshAccountTransfersCommandHandler> logger)
    {
        _validator = validator;
        _paymentService = paymentService;
        _transferRepository = transferRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RefreshAccountTransfersCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        try
        {
            _logger.LogInformation($"Getting account transfers from payment api for AccountId = '{request.ReceiverAccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

            var paymentTransfers = await _paymentService.GetAccountTransfers(request.PeriodEnd, request.ReceiverAccountId, request.CorrelationId);

            _logger.LogInformation($"Retrieved payment transfers from payment api for AccountId = '{request.ReceiverAccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

            //Handle multiple transfers for the same account, period end and commitment ID by grouping them together
            //This can happen if delivery months are different by collection months are not for payments
            var transfers = paymentTransfers.GroupBy(t => new { t.SenderAccountId, t.ReceiverAccountId, CommitmentId = t.ApprenticeshipId, t.PeriodEnd })
                .Select(g =>
                {
                    var firstGroupItem = g.First();

                    return new AccountTransfer
                    {
                        PeriodEnd = firstGroupItem.PeriodEnd,
                        Amount = g.Sum(x => x.Amount),
                        ApprenticeshipId = firstGroupItem.ApprenticeshipId,
                        ReceiverAccountId = firstGroupItem.ReceiverAccountId,
                        SenderAccountId = firstGroupItem.SenderAccountId,
                        Type = firstGroupItem.Type
                    };
                }).ToArray();

            _logger.LogInformation($"Retrieved {transfers.Length} grouped account transfers from payment api for AccountId = '{request.ReceiverAccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

            await _transferRepository.CreateAccountTransfers(transfers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Could not process transfers for Account Id {request.ReceiverAccountId} and Period End {request.PeriodEnd}, CorrelationId = {request.CorrelationId}");
            throw;
        }

        _logger.LogInformation($"Refresh account transfers handler complete for AccountId = '{request.ReceiverAccountId}' and PeriodEnd = '{request.PeriodEnd}' CorrelationId: {request.CorrelationId}");

        return Unit.Value;
    }
}
