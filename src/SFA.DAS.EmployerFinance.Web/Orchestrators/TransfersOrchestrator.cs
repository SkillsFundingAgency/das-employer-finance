﻿using System;
using System.Threading.Tasks;
using SFA.DAS.Authorization.EmployerUserRoles.Options;
using SFA.DAS.Authorization.Services;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.HashingService;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.Authorization.EmployerFeatures.Models;
using SFA.DAS.EAS.Account.Api.Client;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators
{
    public class TransfersOrchestrator
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHashingService _hashingService;
        private readonly IManageApprenticeshipsService _manageApprenticeshipsService;
        private readonly IAccountApiClient _accountApiClient;
        private readonly IFeatureTogglesService<EmployerFeatureToggle> _featureTogglesService;

        protected TransfersOrchestrator()
        {

        }

        public TransfersOrchestrator(
            IAuthorizationService authorizationService,
            IHashingService hashingService,
            IManageApprenticeshipsService manageApprenticeshipsService,
            IAccountApiClient accountApiClient,
            IFeatureTogglesService<EmployerFeatureToggle> featureTogglesService)
        {
            _authorizationService = authorizationService;
            _hashingService = hashingService;
            _manageApprenticeshipsService = manageApprenticeshipsService;
            _accountApiClient = accountApiClient;
            _featureTogglesService = featureTogglesService;
        }

        public async Task<OrchestratorResponse<IndexViewModel>> GetIndexViewModel(string hashedAccountId)
        {
            var accountId = _hashingService.DecodeValue(hashedAccountId);
            var indexTask = _manageApprenticeshipsService.GetIndex(accountId);
            var accountDetail = _accountApiClient.GetAccount(hashedAccountId);

            var renderCreateTransfersPledgeButtonTask = _authorizationService.IsAuthorizedAsync(EmployerUserRole.OwnerOrTransactor);
            var renderApplicationListButton = _featureTogglesService.GetFeatureToggle("ApplicationList");

            await Task.WhenAll(indexTask, renderCreateTransfersPledgeButtonTask, accountDetail);

            Enum.TryParse(accountDetail.Result.ApprenticeshipEmployerType, true, out ApprenticeshipEmployerType employerType);

            if (indexTask.Result.IsTransferReceiver || employerType == ApprenticeshipEmployerType.NonLevy)
            {
                return new OrchestratorResponse<IndexViewModel>()
                {
                    Data = new IndexViewModel
                    {
                        CanViewPledgesSection = false
                    }
                };
            }

            return new OrchestratorResponse<IndexViewModel>
            {
                Data = new IndexViewModel
                {
                    CanViewPledgesSection = true,
                    PledgesCount = indexTask.Result.PledgesCount,
                    ApplicationsCount = indexTask.Result.ApplicationsCount,
                    IsTransferReceiver = indexTask.Result.IsTransferReceiver,
                    IsTransferSender = indexTask.Result.IsTransferSender,
                    RenderCreateTransfersPledgeButton = renderCreateTransfersPledgeButtonTask.Result,
                    RenderApplicationListButton = renderApplicationListButton.IsEnabled
                }
            };
        }
    }
}