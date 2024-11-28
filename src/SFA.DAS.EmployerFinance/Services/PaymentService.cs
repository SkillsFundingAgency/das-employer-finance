using System.Text.Json;
using AutoMapper;
using SFA.DAS.Caches;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.ApprenticeshipCourse;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Types;
using AccountTransfer = SFA.DAS.EmployerFinance.Models.Transfers.AccountTransfer;
using Payment = SFA.DAS.Provider.Events.Api.Types.Payment;

namespace SFA.DAS.EmployerFinance.Services;

public class PaymentService(
    IPaymentsEventsApiClient paymentsEventsApiClient,
    ICommitmentsV2ApiClient commitmentsV2ApiClient,
    IApprenticeshipInfoServiceWrapper apprenticeshipInfoService,
    IMapper mapper,
    ILogger<PaymentService> logger,
    IInProcessCache inProcessCache,
    IProviderService providerService)
    : IPaymentService
{
    public async Task<ICollection<PaymentDetails>> GetAccountPayments(string periodEnd, long employerAccountId, Guid correlationId)
    {
        var populatedPayments = new List<PaymentDetails>();

        var totalPages = 1;

        for (var index = 1; index <= totalPages; index++)
        {
            var payments = await GetPaymentsPage(employerAccountId, periodEnd, index).ConfigureAwait(false);

            if (payments == null)
            {
                continue;
            }

            totalPages = payments.TotalNumberOfPages;

            var paymentDetails = payments.Items.Select(mapper.Map<PaymentDetails>);

            populatedPayments.AddRange(paymentDetails);

            logger.LogInformation("Populated payments page {Index} of {TotalPages} for AccountId = {EmployerAccountId}, PeriodEnd={PeriodEnd}, correlationId = {CorrelationId}",
                index, totalPages, employerAccountId, periodEnd, correlationId);
        }

        await Parallel.ForEachAsync(populatedPayments, (details, _) =>
        {
            details.PeriodEnd = periodEnd;
            return ValueTask.CompletedTask;
        });

        return populatedPayments;
    }

    public async Task<PaymentDetails> AddSinglePaymentDetailsMetadata(long employerAccountId, PaymentDetails paymentDetails)
    {
        logger.LogInformation("{MethodName}: Starting processing for {PaymentId} - {ApprenticeshipId} - {PeriodEnd}.", nameof(AddSinglePaymentDetailsMetadata), paymentDetails.Id, paymentDetails.ApprenticeshipId, paymentDetails.PeriodEnd);

        var providerDetailsTask = providerService.Get(paymentDetails.Ukprn);
        var apprenticeshipTask = GetApprenticeship(employerAccountId, paymentDetails.ApprenticeshipId);

        await Task.WhenAll(providerDetailsTask, apprenticeshipTask);

        var apprenticeship = apprenticeshipTask.Result;
        var providerDetails = providerDetailsTask.Result;

        logger.LogInformation("{MethodName}: Provider {Provider}", nameof(AddSinglePaymentDetailsMetadata), JsonSerializer.Serialize(providerDetails));
        logger.LogInformation("{MethodName}: Apprenticeship {Apprenticeship}", nameof(AddSinglePaymentDetailsMetadata), JsonSerializer.Serialize(apprenticeship));

        if (apprenticeship != null)
        {
            paymentDetails.ApprenticeName = $"{apprenticeship.FirstName} {apprenticeship.LastName}";
            paymentDetails.CourseStartDate = apprenticeship.StartDate;
        }
        else
        {
            logger.LogInformation("{MethodName}: Apprentice not found for {PaymentId} - {ApprenticeshipId}", nameof(AddSinglePaymentDetailsMetadata), paymentDetails.Id, paymentDetails.ApprenticeshipId);
        }

        paymentDetails.ProviderName = providerDetails?.Name;
        paymentDetails.IsHistoricProviderName = providerDetails?.IsHistoricProviderName ?? false;

        await GetCourseDetails(paymentDetails);

        logger.LogInformation("{MethodName}: Completed processing for {PaymentId} - {ApprenticeshipId}", nameof(AddSinglePaymentDetailsMetadata), paymentDetails.Id, paymentDetails.ApprenticeshipId);

        return paymentDetails;
    }

    public async Task<IEnumerable<AccountTransfer>> GetAccountTransfers(string periodEnd, long receiverAccountId, Guid correlationId)
    {
        var pageOfTransfers = await paymentsEventsApiClient.GetTransfers(periodEnd, receiverAccountId: receiverAccountId);

        var transfers = new List<AccountTransfer>();

        foreach (var item in pageOfTransfers.Items)
        {
            transfers.Add(new AccountTransfer
            {
                SenderAccountId = item.SenderAccountId,
                ReceiverAccountId = item.ReceiverAccountId,
                PeriodEnd = periodEnd,
                Amount = item.Amount,
                ApprenticeshipId = item.CommitmentId,
                Type = item.Type.ToString(),
                RequiredPaymentId = item.RequiredPaymentId
            });
        }

        return transfers;
    }

    private async Task GetCourseDetails(PaymentDetails payment)
    {
        payment.CourseName = string.Empty;

        if (payment.StandardCode is > 0)
        {
            var standard = await GetStandard(payment.StandardCode.Value);

            payment.CourseName = standard?.CourseName;
            payment.CourseLevel = standard?.Level;
        }
        else if (payment.FrameworkCode.HasValue && payment.FrameworkCode > 0)
        {
            await GetFrameworkCourseDetails(payment);
        }
        else
        {
            logger.LogWarning("No framework code or standard code set on payment. Cannot get course details. PaymentId: {PaymentId}", payment.Id);
        }
    }

    private async Task GetFrameworkCourseDetails(PaymentDetails payment)
    {
        if (payment.FrameworkCode.HasValue && payment.ProgrammeType.HasValue && payment.PathwayCode.HasValue)
        {
            var framework = await GetFramework(
                payment.FrameworkCode.Value,
                payment.ProgrammeType.Value,
                payment.PathwayCode.Value);

            payment.CourseName = framework?.FrameworkName;
            payment.CourseLevel = framework?.Level;
            payment.PathwayName = framework?.PathwayName;
        }
    }

    private async Task<GetApprenticeshipResponse> GetApprenticeship(long employerAccountId, long apprenticeshipId)
    {
        logger.LogInformation("Getting apprenticeship details for EmployerAccountId: {EmployerId} and ApprenticeshipId: {ApprenticeshipId}", employerAccountId, apprenticeshipId);

        try
        {
            return await commitmentsV2ApiClient.GetApprenticeship(apprenticeshipId);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to get Apprenticeship with Employer Account ID {EmployerAccountId} and apprenticeship ID {ApprenticeshipId} from commitments API.", employerAccountId, apprenticeshipId);
        }

        return null;
    }

    private async Task<PageOfResults<Payment>> GetPaymentsPage(long employerAccountId, string periodEnd, int page)
    {
        try
        {
            return await paymentsEventsApiClient.GetPayments(periodEnd, employerAccountId.ToString(), page, null);
        }
        catch (WebException webException)
        {
            logger.LogError(webException, "Unable to get payment information for {PeriodEnd} accountId {EmployerAccountId}", periodEnd, employerAccountId);
        }

        return null;
    }

    private async Task<Standard> GetStandard(long standardCode)
    {
        try
        {
            var standardsView = inProcessCache.Get<StandardsView>(nameof(StandardsView));

            if (standardsView != null)
            {
                return standardsView.Standards?.SingleOrDefault(s => s.Code.Equals(standardCode));
            }

            standardsView = await apprenticeshipInfoService.GetStandardsAsync();

            if (standardsView != null)
            {
                inProcessCache.Set(nameof(StandardsView), standardsView, new TimeSpan(1, 0, 0));
            }

            return standardsView?.Standards?.SingleOrDefault(s => s.Code.Equals(standardCode));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not get standards from apprenticeship API.");
        }

        return null;
    }

    private async Task<Framework> GetFramework(int frameworkCode, int programType, int pathwayCode)
    {
        try
        {
            var frameworksView = inProcessCache.Get<FrameworksView>(nameof(FrameworksView));

            if (frameworksView != null)
            {
                return frameworksView.Frameworks.SingleOrDefault(f =>
                    f.FrameworkCode.Equals(frameworkCode) &&
                    f.ProgrammeType.Equals(programType) &&
                    f.PathwayCode.Equals(pathwayCode));
            }

            frameworksView = await apprenticeshipInfoService.GetFrameworksAsync();

            if (frameworksView != null)
            {
                inProcessCache.Set(nameof(FrameworksView), frameworksView, new TimeSpan(1, 0, 0));
            }

            return frameworksView?.Frameworks.SingleOrDefault(f =>
                f.FrameworkCode.Equals(frameworkCode) &&
                f.ProgrammeType.Equals(programType) &&
                f.PathwayCode.Equals(pathwayCode));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not get frameworks from apprenticeship API.");
        }

        return null;
    }
}