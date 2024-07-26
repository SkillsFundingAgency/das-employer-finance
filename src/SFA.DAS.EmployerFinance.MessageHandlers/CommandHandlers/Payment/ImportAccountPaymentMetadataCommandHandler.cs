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
        logger.LogInformation("{HandlerName} started", nameof(ImportAccountPaymentMetadataCommandHandler));

        logger.LogInformation("Processing refresh payment metadata for {AccountId} PeriodEnd: {PaymentId}", message.AccountId, message.PaymentId);
        
         await mediator.Send(new RefreshPaymentMetadataCommand
        {
            AccountId = message.AccountId,
            PeriodEndRef = message.PeriodEndRef,
            PaymentId = message.PaymentId
        }).ConfigureAwait(false);

        logger.LogInformation("{HandlerName} completed", nameof(ImportAccountPaymentMetadataCommandHandler));
    }
}
