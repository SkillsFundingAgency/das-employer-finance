using SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions;
using SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers.Payment;

public class ImportAccountPaymentsCommandHandler(
    IMediator mediator,
    ILogger<ImportAccountPaymentsCommandHandler> logger)
    : IHandleMessages<ImportAccountPaymentsCommand>
{
    public async Task Handle(ImportAccountPaymentsCommand message, IMessageHandlerContext context)
    {
        var correlationId = Guid.NewGuid();

        var paymentsResponse = await RefreshPaymentData(message, context, correlationId);

        await RefreshAccountTransfers(message, correlationId);

        await CreateTransferTransactions(message, correlationId);

        await ImportAccountPaymentMetadata(message, context, paymentsResponse);
    }

    private async Task ImportAccountPaymentMetadata(ImportAccountPaymentsCommand message, IMessageHandlerContext context, RefreshPaymentDataResponse paymentsResponse)
    {
        await Parallel.ForEachAsync(paymentsResponse.PaymentDetails,
            async (payment, _) =>
            {
                await SendImportAccountPaymentMetadataCommand(message, context, payment);
            });
    }

    private async Task SendImportAccountPaymentMetadataCommand(ImportAccountPaymentsCommand message, IMessageHandlerContext context, PaymentDetails payment)
    {
        logger.LogInformation(
            "Creating {MessageType} message for {AccountId} - {PaymentId}",
            nameof(ImportAccountPaymentMetadataCommand),
            message.AccountId,
            payment.Id);

        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();
        sendOptions.SetMessageId($"{nameof(ImportAccountPaymentsCommand)}-{message.PeriodEndRef}-{message.AccountId}-{payment.Id}");

        await context
            .Send(new ImportAccountPaymentMetadataCommand
            {
                AccountId = message.AccountId,
                PaymentId = payment.Id,
                PeriodEndRef = payment.PeriodEnd
            }, sendOptions);
    }

    private async Task CreateTransferTransactions(ImportAccountPaymentsCommand message, Guid correlationId)
    {
        logger.LogInformation($"Processing create account transfer transactions command for AccountId:{message.AccountId} PeriodEnd:{message.PeriodEndRef}, CorrelationId: {correlationId}");

        await mediator.Send(new CreateTransferTransactionsCommand
        {
            ReceiverAccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        }).ConfigureAwait(false);
    }

    private async Task RefreshAccountTransfers(ImportAccountPaymentsCommand message, Guid correlationId)
    {
        logger.LogInformation($"Processing refresh account transfers command for AccountId:{message.AccountId} PeriodEnd:{message.PeriodEndRef}, CorrelationId: {correlationId}");

        await mediator.Send(new RefreshAccountTransfersCommand
        {
            ReceiverAccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        }).ConfigureAwait(false);
    }

    private async Task<RefreshPaymentDataResponse> RefreshPaymentData(ImportAccountPaymentsCommand message, IMessageHandlerContext context, Guid correlationId)
    {
        logger.LogInformation($"Processing refresh payment command for Account ID: {message.AccountId} PeriodEnd: {message.PeriodEndRef} CorrelationId: {correlationId}, MessageId: {context.MessageId}");

        return await mediator.Send(new RefreshPaymentDataCommand
        {
            AccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        }).ConfigureAwait(false);
    }
}