using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators;

public class TransfersOrchestrator(
    IEmployerAccountAuthorisationHandler authorizationService,
    IEncodingService encodingService,
    ITransfersService transfersService,
    IAccountApiClient accountApiClient,
    EmployerFinanceConfiguration configuration)
{
    private const int minimumTransferFunds = 2000;

    public async Task<OrchestratorResponse<IndexViewModel>> GetIndexViewModel(string hashedAccountId)
    {
        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var indexTask = transfersService.GetCounts(accountId);
        var accountDetail = accountApiClient.GetAccount(hashedAccountId);

        var renderCreateTransfersPledgeButton = await authorizationService.CheckUserAccountAccess(EmployerUserRole.Transactor);

        await Task.WhenAll(indexTask, accountDetail);

        Enum.TryParse(accountDetail.Result.ApprenticeshipEmployerType, true, out ApprenticeshipEmployerType employerType);

        var estimatedRemainingAllowance = accountDetail.Result.StartingTransferAllowance - indexTask.Result.CurrentYearEstimatedCommittedSpend;
        var exceedsMinimumTransferFundRequirement = estimatedRemainingAllowance >= minimumTransferFunds;

        return new OrchestratorResponse<IndexViewModel>
        {
            Data = new IndexViewModel
            {
                IsLevyEmployer = employerType == ApprenticeshipEmployerType.Levy,
                PledgesCount = indexTask.Result.PledgesCount,
                ApplicationsCount = indexTask.Result.ApplicationsCount,
                RenderCreateTransfersPledgeButton = renderCreateTransfersPledgeButton,
                StartingTransferAllowance = accountDetail.Result.StartingTransferAllowance,
                FinancialYearString = DateTime.UtcNow.Year.ToString(),
                HashedAccountID = hashedAccountId,
                CurrentYearEstimatedSpend = indexTask.Result.CurrentYearEstimatedCommittedSpend,
                HasMinimumTransferFunds = exceedsMinimumTransferFundRequirement,
                TransferAllowancePercentage = configuration.TransferAllowancePercentage * 100
            }
        };
    }
}