using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Attributes;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.Web.Attributes;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Infrastructure;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [SetNavigationSection(NavigationSection.AccountsFinance)]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("accounts/{HashedAccountId}/transfers/connections")]
    public class TransferConnectionsController : Controller
    {
        private readonly ILogger<TransferConnectionsController> _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;

        public TransferConnectionsController(ILogger<TransferConnectionsController> logger, IMapper mapper, IMediator mediator, IEncodingService encodingService)
        {
            _logger = logger;
            _mapper = mapper;
            _mediator = mediator;
            _encodingService = encodingService;
        }

        [HttpGet]
        [Route("",Name = RouteNames.TransferConnectionsIndex)]
        public async Task<IActionResult> Index([FromRoute]string hashedAccountId)
        {
            var response = await _mediator.Send(new GetEmployerAccountDetailByHashedIdQuery
            {
                HashedAccountId = hashedAccountId
            });

            var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
//This cant be done with Task.WhenAll
            var transferAllowanceTask = await TransferAllowance(accountId);
            var transferConnectionInvitationAuthorizationTask =await TransferConnectionInvitationAuthorization(accountId);
            var transferConnectionInvitationsTask =  await TransferConnectionInvitations(accountId);
            var transferRequestsTask = await TransferRequests(accountId);

            var model = new TransferViewModel
            {
                ApprenticeshipEmployerType = response.AccountDetail.ApprenticeshipEmployerType,
                TransferAllowanceViewModel = transferAllowanceTask,
                TransferConnectionInvitationAuthorizationViewModel = transferConnectionInvitationAuthorizationTask,
                TransferConnectionInvitationsViewModel = transferConnectionInvitationsTask,
                TransferRequest = transferRequestsTask,
            };
            
            return View(model);
        }

        
        public async Task<TransferAllowanceViewModel> TransferAllowance(long accountId)
        {
            var response = await _mediator.Send(new GetTransferAllowanceQuery
            {
                AccountId = accountId
            });
            return _mapper.Map<TransferAllowanceViewModel>(response);
        }

        
        public async Task<TransferConnectionInvitationAuthorizationViewModel> TransferConnectionInvitationAuthorization(long accountId)
        {
            var response = await _mediator.Send(new GetTransferConnectionInvitationAuthorizationQuery
            {
                AccountId = accountId
            });
            return _mapper.Map<TransferConnectionInvitationAuthorizationViewModel>(response);
        }

        public async Task<TransferConnectionInvitationsViewModel> TransferConnectionInvitations(long accountId)
        {
            var response = await _mediator.Send(new GetTransferConnectionInvitationsQuery
            {
                AccountId = accountId
            });
            return _mapper.Map<TransferConnectionInvitationsViewModel>(response);
            
        }

        public async Task<TransferRequestsViewModel> TransferRequests(long accountId)
        {
            try
            {
                var response = await _mediator.Send(new GetTransferRequestsQuery
                {
                    AccountId = accountId
                });
                return _mapper.Map<TransferRequestsViewModel>(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get transfer requests");

                return null;
            }
        }
    }
}