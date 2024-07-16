using System.Threading;
using Azure.Messaging.ServiceBus;
using Dasync.Collections;
using SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions;
using SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;

public class ImportAccountPaymentsCommandHandler(
    IMediator mediator,
    ILogger<ImportAccountPaymentsCommandHandler> logger,
    EmployerFinanceConfiguration employerFinanceConfiguration)
    : IHandleMessages<ImportAccountPaymentsCommand>
{
    private readonly ServiceBusClient _serviceBusClient = new(employerFinanceConfiguration.ServiceBusConnectionString);
    private readonly TimeSpan _renewalInterval = TimeSpan.FromSeconds(30);
    
    public async Task Handle(ImportAccountPaymentsCommand message, IMessageHandlerContext context)
    {
        var messageReceiver = _serviceBusClient.CreateReceiver("SFA.DAS.EmployerFinance.MessageHandlers");

        using var cts = new CancellationTokenSource();

        var lockRenewalTask = RenewLockAsync(messageReceiver, cts.Token);

        try
        {
            await ProcessMessageAsync(message, context.MessageId);
        }
        finally
        {
            cts.Cancel();
            await lockRenewalTask;
        }
    }
    
    private async Task RenewLockAsync(ServiceBusReceiver messageReceiver, CancellationToken cancellationToken)
    {
        try
        {
            var receivedMessage = await messageReceiver
                .ReceiveMessagesAsync(cancellationToken)
                .FirstAsync(token: cancellationToken);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_renewalInterval, cancellationToken);
                await messageReceiver.RenewMessageLockAsync(receivedMessage, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Expected exception when the task is canceled
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lock renewal failed: {ex.Message}");
        }
    }

    private async Task ProcessMessageAsync(ImportAccountPaymentsCommand message, string messageId)
    {
        var correlationId = Guid.NewGuid();
        logger.LogInformation($"Processing refresh payment command for Account ID: {message.AccountId} PeriodEnd: {message.PeriodEndRef} CorrelationId: {correlationId}, MessageId: {messageId}");
        
        await mediator.Send(new RefreshPaymentDataCommand
        {
            AccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        });

        logger.LogInformation($"Processing refresh account transfers command for AccountId:{message.AccountId} PeriodEnd:{message.PeriodEndRef}, CorrelationId: {correlationId}");

        await mediator.Send(new RefreshAccountTransfersCommand
        {
            ReceiverAccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        });

        logger.LogInformation($"Processing create account transfer transactions command for AccountId:{message.AccountId} PeriodEnd:{message.PeriodEndRef}");

        await mediator.Send(new CreateTransferTransactionsCommand
        {
            ReceiverAccountId = message.AccountId,
            PeriodEnd = message.PeriodEndRef,
            CorrelationId = correlationId
        });
    }
}