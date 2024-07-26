using SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions;
using SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers.Payment;

public class ImportAccountPaymentsCommandHandler(
    IMediator mediator,
    ILogger<ImportAccountPaymentsCommandHandler> logger)
    : IHandleMessages<ImportAccountPaymentsCommand>
{
    public async Task Handle(ImportAccountPaymentsCommand message, IMessageHandlerContext context)
    {
        var correlationId = Guid.NewGuid();

        logger.LogInformation($"Processing refresh payment command for Account ID: {message.AccountId} PeriodEnd: {message.PeriodEndRef} CorrelationId: {correlationId}, MessageId: {context.MessageId}");
        
        var paymentsResponse = await mediator.Send(new RefreshPaymentDataCommand
        {
            AccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        }).ConfigureAwait(false);

        logger.LogInformation($"Processing refresh account transfers command for AccountId:{message.AccountId} PeriodEnd:{message.PeriodEndRef}, CorrelationId: {correlationId}");

        await mediator.Send(new RefreshAccountTransfersCommand
        {
            ReceiverAccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        }).ConfigureAwait(false);

        logger.LogInformation($"Processing create account transfer transactions command for AccountId:{message.AccountId} PeriodEnd:{message.PeriodEndRef}, CorrelationId: {correlationId}");

        await mediator.Send(new CreateTransferTransactionsCommand
        {
            ReceiverAccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        }).ConfigureAwait(false);
        
        await Parallel.ForEachAsync(paymentsResponse.PaymentDetails,
            async (payment, _) =>
            {
                logger.LogInformation(
                    "Creating {MessageType} message for {AccountId} - {PaymentId}", 
                    nameof(ImportAccountPaymentMetadataCommand),
                    message.AccountId,
                    payment.Id);

                var sendOptions = new SendOptions();
                sendOptions.RouteToThisEndpoint();
                sendOptions.SetMessageId(
                    $"{nameof(ImportAccountPaymentsCommand)}-{message.PeriodEndRef}-{message.AccountId}-{payment.Id}");

                await context
                    .Send(new ImportAccountPaymentMetadataCommand
                    {
                        AccountId = message.AccountId,
                        PaymentId = payment.Id
                    }, sendOptions)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);
    }
}
