using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.EmployerFinance.Commands.ApproveTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Commands.DeleteTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Commands.RejectTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Commands.SendTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Queries.GetApprovedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Queries.GetLatestPendingReceivedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Queries.GetReceivedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Queries.GetRejectedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Queries.GetSentTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Queries.SendTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Attributes;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using EmployerUserRole = SFA.DAS.Authorization.EmployerUserRoles.Options.EmployerUserRole;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    //[DasAuthorize("EmployerFeature.TransferConnectionRequests", EmployerUserRole.Any)]
    //TODO MAC-192 - this should be restricted on some actions to owner role
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("accounts/{HashedAccountId}/transfers/connections/requests")]
    public class TransferConnectionInvitationsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUrlActionHelper _urlHelper;

        public TransferConnectionInvitationsController(IMapper mapper, IMediator mediator, IUrlActionHelper urlHelper)
        {
            _mapper = mapper;
            _mediator = mediator;
            _urlHelper = urlHelper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [ImportModelStateFromTempData]
        [Route("start")]
        public IActionResult Start()
        {
            return View(new StartTransferConnectionInvitationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]        
        [Route("start")]
        public async Task<IActionResult> Start(StartTransferConnectionInvitationViewModel model)
        {
            await _mediator.Send(new SendTransferConnectionInvitationQuery
            {
                ReceiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId,
                AccountId = model.AccountId
            });
            return RedirectToAction("Send", new { receiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId });
        }

        [HttpNotFoundForNullModel]
        [ImportModelStateFromTempData]
        [Route("send")]
        public async Task<IActionResult> Send(SendTransferConnectionInvitationQuery query)
        {
            var response = await _mediator.Send(query);
            var model = _mapper.Map<SendTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        [Route("send")]
        public async Task<IActionResult> Send(SendTransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "Confirm":
                    var transferConnectionInvitationId = await _mediator.Send(new SendTransferConnectionInvitationCommand
                    {
                        AccountId = model.AccountId,
                        ReceiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId,
                        UserRef = model.UserRef
                    });
                    return RedirectToAction("Sent", new { transferConnectionInvitationId });
                case "ReEnterAccountId":
                    return RedirectToAction("Start");
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpNotFoundForNullModel]
        [ImportModelStateFromTempData]
        [Route("{transferConnectionInvitationId}/sent")]
        public async Task<IActionResult> Sent(GetSentTransferConnectionInvitationQuery query)
        {
            var response = await _mediator.Send(query);
            var model = _mapper.Map<SentTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/sent")]
        public IActionResult Sent(SentTransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "GoToTransfersPage":
                    return RedirectToAction("Index", "TransferConnections");
                case "GoToHomepage":
                    return Redirect(_urlHelper.EmployerAccountsAction("teams"));
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpNotFoundForNullModel]
        [ImportModelStateFromTempData]
        [Route("{transferConnectionInvitationId}/receive")]
        public async Task<IActionResult> Receive(GetReceivedTransferConnectionInvitationQuery query)
        {
            var response = await _mediator.Send(query);
            var model = _mapper.Map<ReceiveTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/receive")]
        public async Task<IActionResult> Receive(ReceiveTransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "Approve":
                    await _mediator.Send(new ApproveTransferConnectionInvitationCommand { AccountId = model.AccountId, UserRef = model.UserRef, TransferConnectionInvitationId = model.TransferConnectionInvitationId });
                    return RedirectToAction("Approved", new { transferConnectionInvitationId = model.TransferConnectionInvitationId });
                case "Reject":
                    await _mediator.Send(new RejectTransferConnectionInvitationCommand { AccountId = model.AccountId, UserRef = model.UserRef, TransferConnectionInvitationId = model.TransferConnectionInvitationId });
                    return RedirectToAction("Rejected", new { transferConnectionInvitationId = model.TransferConnectionInvitationId });
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpNotFoundForNullModel]
        [ImportModelStateFromTempData]
        [Route("{transferConnectionInvitationId}/approved")]
        public async Task<IActionResult> Approved(GetApprovedTransferConnectionInvitationQuery query)
        {
            var response = await _mediator.Send(query);
            var model = _mapper.Map<ApprovedTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/approved")]
        public IActionResult Approved(ApprovedTransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "GoToApprenticesPage":
                    return Redirect(_urlHelper.EmployerCommitmentsV2Action(string.Empty));
                case "GoToHomepage":
                    return Redirect(_urlHelper.EmployerAccountsAction("teams"));
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpNotFoundForNullModel]
        [ImportModelStateFromTempData]
        [Route("{transferConnectionInvitationId}/rejected")]
        public async Task<IActionResult> Rejected(GetRejectedTransferConnectionInvitationQuery query)
        {
            var response = await _mediator.Send(query);
            var model = _mapper.Map<RejectedTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/rejected")]
        public async Task<IActionResult> Rejected(RejectedTransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "Confirm":
                    await _mediator.Send(new DeleteTransferConnectionInvitationCommand
                    {
                        AccountId = model.AccountId,
                        TransferConnectionInvitationId = model.TransferConnectionInvitationId,
                        UserRef = model.UserRef
                    });
                    return RedirectToAction("Deleted");
                case "GoToTransfersPage":
                    return RedirectToAction("Index", "TransferConnections");
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpNotFoundForNullModel]
        [Route("{transferConnectionInvitationId}/details")]
        public async Task<IActionResult> Details(GetTransferConnectionInvitationQuery query)
        {
            var response = await _mediator.Send(query);
            var model = _mapper.Map<TransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/details")]
        public async Task<IActionResult> Details(TransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "Confirm":
                    await _mediator.Send(new DeleteTransferConnectionInvitationCommand
                    {
                        AccountId = model.AccountId,
                        TransferConnectionInvitationId = model.TransferConnectionInvitationId,
                        UserRef = model.UserRef
                    });
                    return RedirectToAction("Deleted");
                case "GoToTransfersPage":
                    return RedirectToAction("Index", "TransferConnections");
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [ImportModelStateFromTempData]
        [Route("{transferConnectionInvitationId}/deleted")]
        public IActionResult Deleted()
        {
            var model = new DeletedTransferConnectionInvitationViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/deleted")]
        public IActionResult Deleted(DeletedTransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "GoToTransfersPage":
                    return RedirectToAction("Index", "TransferConnections");
                case "GoToHomepage":
                    return Redirect(_urlHelper.EmployerAccountsAction("teams"));
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpGet]
        [Route("outstanding")]
        public async Task<IActionResult> Outstanding(GetLatestPendingReceivedTransferConnectionInvitationQuery query)
        {
            var response = await _mediator.Send(query);

            return response.TransferConnectionInvitation == null
                ? RedirectToAction("Index", "TransferConnections")
                : RedirectToAction("Receive", new { transferConnectionInvitationId = response.TransferConnectionInvitation.Id });
        }
    }
}