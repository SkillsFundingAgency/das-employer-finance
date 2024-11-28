using SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers.Payment;

public class ImportPaymentsCommandHandler(
    IPaymentsEventsApiClient paymentsEventsApiClient,
    IMediator mediator,
    ILogger<ImportPaymentsCommandHandler> logger)
    : IHandleMessages<ImportPaymentsCommand>
{
    public async Task Handle(ImportPaymentsCommand message, IMessageHandlerContext context)
    {
        logger.LogInformation("Calling Payments API");

        var periodEnds = await paymentsEventsApiClient.GetPeriodEnds();

        var result = await mediator.Send(new GetPeriodEndsRequest());

        var periodsToProcess = periodEnds
            .Where(pe => result.CurrentPeriodEnds.All(existing => existing.PeriodEndId != pe.Id))
            .ToList();

        if (!periodsToProcess.Any())
        {
            logger.LogInformation("No Period Ends to Process");
            return;
        }

        foreach (var paymentsPeriodEnd in periodsToProcess)
        {
            await ProcessPaymentPeriod(paymentsPeriodEnd, context);
        }
    }

    private async Task ProcessPaymentPeriod(PeriodEnd paymentsPeriodEnd, IMessageHandlerContext context)
    {
        var periodEnd = MapToDbPaymentPeriod(paymentsPeriodEnd);

        logger.LogInformation("Creating period end {PeriodEndId}", periodEnd.PeriodEndId);

        await mediator.Send(new CreateNewPeriodEndCommand { NewPeriodEnd = periodEnd });

        if (!periodEnd.AccountDataValidAt.HasValue || !periodEnd.CommitmentDataValidAt.HasValue)
        {
            return;
        }

        logger.LogInformation("Creating process period end queue message for period end ref: '{Id}'", paymentsPeriodEnd.Id);

        await context.SendLocal(new ProcessPeriodEndPaymentsCommand
        {
            PeriodEndRef = paymentsPeriodEnd.Id
        });
    }

    private static Models.Payments.PeriodEnd MapToDbPaymentPeriod(PeriodEnd paymentsPeriodEnd)
    {
        return new Models.Payments.PeriodEnd
        {
            PeriodEndId = paymentsPeriodEnd.Id,
            CalendarPeriodMonth = paymentsPeriodEnd.CalendarPeriod?.Month ?? 0,
            CalendarPeriodYear = paymentsPeriodEnd.CalendarPeriod?.Year ?? 0,
            CompletionDateTime = paymentsPeriodEnd.CompletionDateTime,
            AccountDataValidAt = paymentsPeriodEnd.ReferenceData?.AccountDataValidAt,
            CommitmentDataValidAt = paymentsPeriodEnd.ReferenceData?.CommitmentDataValidAt,
            PaymentsForPeriod = paymentsPeriodEnd.Links?.PaymentsForPeriod ?? string.Empty
        };
    }
}