using SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions;
using SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;

public class ImportAccountPaymentsCommandHandler(
    IMediator mediator,
    ILogger<ImportAccountPaymentsCommandHandler> logger)
    : IHandleMessages<ImportAccountPaymentsCommand>
{
    public async Task Handle(ImportAccountPaymentsCommand message, IMessageHandlerContext context)
    {
        var correlationId = Guid.NewGuid();

        logger.LogInformation($"Processing refresh payment command for Account ID: {message.AccountId} PeriodEnd: {message.PeriodEndRef} CorrelationId: {correlationId}, MessageId: {context.MessageId}");
        
        await mediator.Send(new RefreshPaymentDataCommand
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

        logger.LogInformation($"Processing create account transfer transactions command for AccountId:{message.AccountId} PeriodEnd:{message.PeriodEndRef}");

        await mediator.Send(new CreateTransferTransactionsCommand
        {
            ReceiverAccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        }).ConfigureAwait(false);
    }
}
