using System;
using System.Security.Claims;
using System.Threading.Tasks;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators
{
    public class TransfersOrchestrator
    {
        private readonly IEmployerAccountAuthorisationHandler _authorizationService;
        private readonly IEncodingService _encodingService;
        private readonly ITransfersService _transfersService;
        private readonly IAccountApiClient _accountApiClient;

        public TransfersOrchestrator(
            IEmployerAccountAuthorisationHandler authorizationService,
            IEncodingService encodingService,
            ITransfersService transfersService,
            IAccountApiClient accountApiClient)
        {
            _authorizationService = authorizationService;
            _encodingService = encodingService;
            _transfersService = transfersService;
            _accountApiClient = accountApiClient;
        }

        public async Task<OrchestratorResponse<IndexViewModel>> GetIndexViewModel(string hashedAccountId)
        {
            var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
            var indexTask = _transfersService.GetCounts(accountId);
            var accountDetail = _accountApiClient.GetAccount(hashedAccountId);

            var renderCreateTransfersPledgeButton = _authorizationService.CheckUserAccountAccess(ClaimsPrincipal.Current, Authentication.EmployerUserRole.Transactor);            

            await Task.WhenAll(indexTask, accountDetail);

            Enum.TryParse(accountDetail.Result.ApprenticeshipEmployerType, true, out ApprenticeshipEmployerType employerType);

            return new OrchestratorResponse<IndexViewModel>
            {
                Data = new IndexViewModel
                {
                    IsLevyEmployer = employerType == ApprenticeshipEmployerType.Levy,
                    PledgesCount = indexTask.Result.PledgesCount,
                    ApplicationsCount = indexTask.Result.ApplicationsCount,
                    RenderCreateTransfersPledgeButton = renderCreateTransfersPledgeButton,                    
                    StartingTransferAllowance = accountDetail.Result.StartingTransferAllowance,
                    FinancialYearString = DateTime.UtcNow.ToFinancialYearString(),
                    HashedAccountID = hashedAccountId
                }
            };
        }

        public async Task<OrchestratorResponse<FinancialBreakdownViewModel>> GetFinancialBreakdownViewModel(string hashedAccountId) 
        {
            var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
            var financialBreakdownTask = _transfersService.GetFinancialBreakdown(accountId);
            var accountDetailTask = _accountApiClient.GetAccount(hashedAccountId);
            await Task.WhenAll(financialBreakdownTask, accountDetailTask);

            return new OrchestratorResponse<FinancialBreakdownViewModel>
            {
                Data = new FinancialBreakdownViewModel
                {
                    TransferConnections = financialBreakdownTask.Result.TransferConnections,
                    HashedAccountID = hashedAccountId,
                    AcceptedPledgeApplications = financialBreakdownTask.Result.AcceptedPledgeApplications + financialBreakdownTask.Result.PledgeOriginatedCommitments,
                    ApprovedPledgeApplications = financialBreakdownTask.Result.ApprovedPledgeApplications,
                    Commitments = financialBreakdownTask.Result.Commitments,
                    PledgeOriginatedCommitments = financialBreakdownTask.Result.PledgeOriginatedCommitments,                    
                    ProjectionStartDate = financialBreakdownTask.Result.ProjectionStartDate,
                    CurrentYearEstimatedSpend = financialBreakdownTask.Result.CurrentYearEstimatedCommittedSpend,
                    NextYearEstimatedSpend = financialBreakdownTask.Result.NextYearEstimatedCommittedSpend,
                    YearAfterNextYearEstimatedSpend = financialBreakdownTask.Result.YearAfterNextYearEstimatedCommittedSpend,
                    StartingTransferAllowance = accountDetailTask.Result.StartingTransferAllowance,
                    FinancialYearString = DateTime.UtcNow.ToFinancialYearString(),
                    NextFinancialYearString = DateTime.UtcNow.AddYears(1).ToFinancialYearString(),
                    YearAfterNextFinancialYearString = DateTime.UtcNow.AddYears(2).ToFinancialYearString(),
                    AmountPledged = financialBreakdownTask.Result.AmountPledged
                }
            };
        }
    }
}