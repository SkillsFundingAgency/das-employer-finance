using SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers.Payment;

public class ImportAccountPaymentMetadataCommandHandler(
    IMediator mediator,
    ILogger<ImportAccountPaymentMetadataCommandHandler> logger)
    : IHandleMessages<ImportAccountPaymentMetadataCommand>
{
    public async Task Handle(ImportAccountPaymentMetadataCommand message, IMessageHandlerContext context)
    {
        logger.LogInformation("{HandlerName} message handler started", nameof(ImportAccountPaymentMetadataCommandHandler));

        logger.LogInformation("Processing refresh payment metadata for {AccountId} PeriodEndRef: {PeriodEnd} with PaymentId: {PaymentId}", message.AccountId,message.PeriodEndRef,  message.PaymentId);

        await mediator.Send(new RefreshPaymentMetadataCommand
        {
            AccountId = message.AccountId,
            PaymentId = message.PaymentId,
            PeriodEndRef = message.PeriodEndRef,
            
        }).ConfigureAwait(false);

        logger.LogInformation("{HandlerName} message handler completed", nameof(ImportAccountPaymentMetadataCommandHandler));
    }
}