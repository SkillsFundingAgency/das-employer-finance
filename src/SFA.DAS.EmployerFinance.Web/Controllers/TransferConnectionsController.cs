using System;
using System.Threading.Tasks;
using System.Security.Claims;
using AutoMapper;
using MediatR;
using SFA.DAS.Authorization.EmployerFeatures.Models;
using SFA.DAS.Authorization.EmployerUserRoles.Options;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.NLog.Logger;
using Microsoft.AspNetCore.Components;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [DasAuthorize(EmployerUserRole.Any)]
    [Route("accounts/{HashedAccountId}/transfers/connections")]
    public class TransferConnectionsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILog _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IFeatureTogglesService<EmployerFeatureToggle> _featureTogglesService;

        public TransferConnectionsController(ILog logger, IMapper mapper, IMediator mediator, IFeatureTogglesService<EmployerFeatureToggle> featureTogglesService)
        {
            _logger = logger;
            _mapper = mapper;
            _mediator = mediator;
            _featureTogglesService = featureTogglesService;
        }

        [Route]
        public async Task<IActionResult> Index(GetEmployerAccountDetailByHashedIdQuery query)
        {
            // redirecting to access denied only when the feature toggle is not enabled, this is not checking
            // whether the feature is authorized, as the view is always displayed when the feature is enabled
            // and the content is different when the feature is not authorized
            var featureToggle = _featureTogglesService.GetFeatureToggle("TransferConnectionRequests");
            if (!featureToggle.IsEnabled)
            {
                return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.AccessDeniedControllerName, 
                    new { hashedAccountId = query.HashedAccountId });
            }

            var response = await _mediator.Send(query);
            ViewBag.ApprenticeshipEmployerType = response.AccountDetail.ApprenticeshipEmployerType;
            return View();
        }

        [ChildActionOnly]
        public IActionResult TransferAllowance(GetTransferAllowanceQuery query)
        {
            var response = Task.Run(() => _mediator.Send(query)).GetAwaiter().GetResult();
            var model = _mapper.Map<TransferAllowanceViewModel>(response);

            return PartialView(model);
        }

        [ChildActionOnly]
        public IActionResult TransferConnectionInvitationAuthorization(GetTransferConnectionInvitationAuthorizationQuery query)
        {
            var response = Task.Run(() => _mediator.Send(query)).GetAwaiter().GetResult();
            var model = _mapper.Map<TransferConnectionInvitationAuthorizationViewModel>(response);

            return PartialView(model);
        }

        [ChildActionOnly]
        public IActionResult TransferConnectionInvitations(GetTransferConnectionInvitationsQuery query)
        {
            var response = Task.Run(() => _mediator.Send(query)).GetAwaiter().GetResult();
            var model = _mapper.Map<TransferConnectionInvitationsViewModel>(response);
            
            return PartialView(model);
        }

        [ChildActionOnly]
        public IActionResult TransferRequests(GetTransferRequestsQuery query)
        {
            try
            {
                var response = Task.Run(() => _mediator.Send(query)).GetAwaiter().GetResult();
                var model = _mapper.Map<TransferRequestsViewModel>(response);

                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get transfer requests");

                return new EmptyResult();
            }
        }
    }
}