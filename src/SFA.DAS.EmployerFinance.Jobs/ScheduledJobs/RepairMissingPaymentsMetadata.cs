using System.Linq;
using NServiceBus;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;

public class RepairMissingPaymentsMetadata(IMessageSession messageSession, IDasLevyRepository levyRepository)
{
   // public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo timer, ILogger logger)
    public async Task Run([TimerTrigger("0 0 11 * * *")] TimerInfo timer, ILogger logger)
    {
        logger.LogInformation("{TypeName}: Starting processing.", nameof(RepairMissingPaymentsMetadata));

        var payments = (await levyRepository.GetPaymentsWithMissingMetadata())?.ToList();

        if (payments == null || payments.Count == 0)
        {
            logger.LogInformation("{TypeName}: No payments found with missing metadata. Exiting.", nameof(RepairMissingPaymentsMetadata));
            return;
        }

        logger.LogInformation("{TypeName}: Found {Count} payments with missing metadata.", nameof(RepairMissingPaymentsMetadata), payments.Count);

        await Parallel.ForEachAsync(payments,
            async (payment, _) => { await SendImportAccountPaymentMetadataCommand(logger, payment); });

        logger.LogInformation("{TypeName}: Completed processing.", nameof(RepairMissingPaymentsMetadata));
    }

    private async Task SendImportAccountPaymentMetadataCommand(ILogger logger, PaymentDetails payment)
    {
        logger.LogInformation(
            "Creating {MessageType} message for {AccountId} - {PaymentId}",
            nameof(ImportAccountPaymentMetadataCommand),
            payment.EmployerAccountId,
            payment.Id);

        var sendOptions = new SendOptions();
        sendOptions.SetMessageId($"{nameof(ImportAccountPaymentMetadataCommand)}-{payment.PeriodEnd}-{payment.EmployerAccountId}-{payment.Id}");

        await messageSession
            .Send(new ImportAccountPaymentMetadataCommand
            {
                AccountId = payment.EmployerAccountId,
                PaymentId = payment.Id,
                PeriodEndRef = payment.PeriodEnd
            }, sendOptions);
    }
}