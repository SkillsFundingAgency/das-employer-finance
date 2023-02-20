using System;
using System.Threading.Tasks;
using SFA.DAS.Authorization.EmployerUserRoles.Options;
using SFA.DAS.Authorization.Services;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators
{
    public class TransfersOrchestrator
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IEncodingService _encodingService;
        private readonly ITransfersService _transfersService;
        private readonly IAccountApiClient _accountApiClient;
        private readonly IDateTimeStringFormatter _dateTimeStringFormatter;

        protected TransfersOrchestrator()
        {
        }

        public TransfersOrchestrator(
            IAuthorizationService authorizationService,
            IEncodingService encodingService,
            ITransfersService transfersService,
            IAccountApiClient accountApiClient,
            IDateTimeStringFormatter dateTimeStringFormatter)
        {
            _authorizationService = authorizationService;
            _encodingService = encodingService;
            _transfersService = transfersService;
            _accountApiClient = accountApiClient;
            _dateTimeStringFormatter = dateTimeStringFormatter;
        }

        public async Task<OrchestratorResponse<IndexViewModel>> GetIndexViewModel(string hashedAccountId)
        {
            var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
            var indexTask = _transfersService.GetCounts(accountId);
            var accountDetail = _accountApiClient.GetAccount(hashedAccountId);

            var renderCreateTransfersPledgeButtonTask = _authorizationService.IsAuthorizedAsync(EmployerUserRole.OwnerOrTransactor);            

            await Task.WhenAll(indexTask, renderCreateTransfersPledgeButtonTask, accountDetail);

            Enum.TryParse(accountDetail.Result.ApprenticeshipEmployerType, true, out ApprenticeshipEmployerType employerType);

            return new OrchestratorResponse<IndexViewModel>
            {
                Data = new IndexViewModel
                {
                    IsLevyEmployer = employerType == ApprenticeshipEmployerType.Levy,
                    PledgesCount = indexTask.Result.PledgesCount,
                    ApplicationsCount = indexTask.Result.ApplicationsCount,
                    RenderCreateTransfersPledgeButton = renderCreateTransfersPledgeButtonTask.Result,                    
                    StartingTransferAllowance = accountDetail.Result.StartingTransferAllowance,
                    FinancialYearString = _dateTimeStringFormatter.FinancialYearStringFor(DateTime.UtcNow),
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
                    FinancialYearString = _dateTimeStringFormatter.FinancialYearStringFor(DateTime.UtcNow),
                    NextFinancialYearString = _dateTimeStringFormatter.FinancialYearStringFor(DateTime.UtcNow.AddYears(1)),
                    YearAfterNextFinancialYearString = _dateTimeStringFormatter.FinancialYearStringFor(DateTime.UtcNow.AddYears(2)),
                    AmountPledged = financialBreakdownTask.Result.AmountPledged
                }
            };
        }
    }
}