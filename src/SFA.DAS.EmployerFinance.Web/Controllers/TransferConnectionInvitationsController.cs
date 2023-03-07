using System;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Attributes;
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
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    //TODO MAC-192 - this should be restricted on some actions to owner role
    [SetNavigationSection(NavigationSection.AccountsFinance)]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("accounts/{HashedAccountId}/transfers/connections/requests")]
    public class TransferConnectionInvitationsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUrlActionHelper _urlHelper;
        private readonly IEncodingService _encodingService;

        public TransferConnectionInvitationsController(IMapper mapper, IMediator mediator, IUrlActionHelper urlHelper, IEncodingService encodingService)
        {
            _mapper = mapper;
            _mediator = mediator;
            _urlHelper = urlHelper;
            _encodingService = encodingService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index([FromRoute]string hashedAccountId)
        {
            return View("Index", hashedAccountId);
        }

        [ImportModelStateFromTempData]
        [Route("start")]
        public IActionResult Start([FromRoute]string hashedAccountId)
        {
            return View(new StartTransferConnectionInvitationViewModel
            {
                HashedAccountId = hashedAccountId
            });
        }

        [HttpPost]
        [ValidateModelState]        
        [Route("start")]
        public async Task<IActionResult> Start([FromRoute]string hashedAccountId, StartTransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Start", model);
            }
            
            await _mediator.Send(new SendTransferConnectionInvitationQuery
            {
                ReceiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId,
                AccountId = _encodingService.Decode(model.HashedAccountId, EncodingType.AccountId)
            });
            return RedirectToAction("Send", new {hashedAccountId, receiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId });
        }

        [HttpNotFoundForNullModel]
        [ImportModelStateFromTempData]
        [Route("send")]
        public async Task<IActionResult> Send([FromRoute]string hashedAccountId, SendTransferConnectionInvitationQuery query)
        {
            query.AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
            var response = await _mediator.Send(query);
            var model = _mapper.Map<SendTransferConnectionInvitationViewModel>(response);
            model.HashedAccountId = hashedAccountId;
            return View(model);
        }

        [HttpPost]
        [ValidateModelState]
        [Route("send")]
        public async Task<IActionResult> Send([FromRoute]string hashedAccountId, SendTransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var query = new SendTransferConnectionInvitationQuery
                {
                    AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                    ReceiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId
                };
                var response = await _mediator.Send(query);
                model = _mapper.Map<SendTransferConnectionInvitationViewModel>(response);
                model.HashedAccountId = hashedAccountId;
                return View("Send", model);
            }
            switch (model.Choice)
            {
                case "Confirm":
                    var transferConnectionInvitationId = await _mediator.Send(new SendTransferConnectionInvitationCommand
                    {
                        AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                        ReceiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId,
                        UserRef = Guid.Parse(User.GetUserId())
                    });
                    return RedirectToAction("Sent", new { transferConnectionInvitationId, hashedAccountId });
                case "ReEnterAccountId":
                    return RedirectToAction("Start", new{hashedAccountId});
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpNotFoundForNullModel]
        [ImportModelStateFromTempData]
        [Route("{transferConnectionInvitationId}/sent")]
        public async Task<IActionResult> Sent([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId)
        {
            var response = await _mediator.Send(new GetSentTransferConnectionInvitationQuery
            {
                AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId)
            });
            var model = _mapper.Map<SentTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
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
        public async Task<IActionResult> Receive([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId)
        {
            var response = await _mediator.Send(new GetReceivedTransferConnectionInvitationQuery
            {
                AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId)
            });
            var model = _mapper.Map<ReceiveTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/receive")]
        public async Task<IActionResult> Receive([FromRoute]string hashedAccountId, ReceiveTransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "Approve":
                    await _mediator.Send(new ApproveTransferConnectionInvitationCommand
                    {
                        AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId), 
                        UserRef = Guid.Parse(User.GetUserId()), 
                        TransferConnectionInvitationId = model.TransferConnectionInvitationId
                    });
                    return RedirectToAction("Approved", new { transferConnectionInvitationId = _encodingService.Encode(Convert.ToInt64(model.TransferConnectionInvitationId), EncodingType.TransferRequestId) });
                case "Reject":
                    await _mediator.Send(new RejectTransferConnectionInvitationCommand
                    {
                        AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId), 
                        UserRef = Guid.Parse(User.GetUserId()), 
                        TransferConnectionInvitationId = model.TransferConnectionInvitationId
                    });
                    return RedirectToAction("Rejected", new { transferConnectionInvitationId = _encodingService.Encode(Convert.ToInt64(model.TransferConnectionInvitationId), EncodingType.TransferRequestId) });
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpNotFoundForNullModel]
        [ImportModelStateFromTempData]
        [Route("{transferConnectionInvitationId}/approved")]
        public async Task<IActionResult> Approved([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId)
        {
            var response = await _mediator.Send(new GetApprovedTransferConnectionInvitationQuery
            {
                AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId)
            });
            var model = _mapper.Map<ApprovedTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/approved")]
        public async Task<IActionResult> Approved([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId, ApprovedTransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var response = await _mediator.Send(new GetApprovedTransferConnectionInvitationQuery
                {
                    AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                    TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId),
                });
                model = _mapper.Map<ApprovedTransferConnectionInvitationViewModel>(response);
                return View(model);
            }
            
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
        [Route("{transferConnectionInvitationId}/rejected")]
        public async Task<IActionResult> Rejected([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId)
        {
            var response = await _mediator.Send(new GetRejectedTransferConnectionInvitationQuery
            {
                AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId),
            });
            var model = _mapper.Map<RejectedTransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/rejected")]
        public async Task<IActionResult> Rejected([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId, RejectedTransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "Confirm":
                    await _mediator.Send(new DeleteTransferConnectionInvitationCommand
                    {
                        AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                        TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId),
                        UserRef = Guid.Parse(User.GetUserId())
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
        public async Task<IActionResult> Details([FromRoute]string hashedAccountId, [FromRoute]string transferConnectionInvitationId)
        {
            var response = await _mediator.Send(new GetTransferConnectionInvitationQuery
            {
                AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId)
            });
            var model = _mapper.Map<TransferConnectionInvitationViewModel>(response);

            return View(model);
        }

        [HttpPost]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/details")]
        public async Task<IActionResult> Details([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId, TransferConnectionInvitationViewModel model)
        {
            switch (model.Choice)
            {
                case "Confirm":
                    await _mediator.Send(new DeleteTransferConnectionInvitationCommand
                    {
                        AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                        TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId),
                        UserRef = Guid.Parse(User.GetUserId())
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
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/deleted")]
        public IActionResult Deleted([FromRoute]string hashedAccountId, DeletedTransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            switch (model.Choice)
            {
                case "GoToTransfersPage":
                    return RedirectToAction("Index", "TransferConnections", new {hashedAccountId});
                case "GoToHomepage":
                    return Redirect(_urlHelper.EmployerAccountsAction("teams"));
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [HttpGet]
        [Route("outstanding")]
        public async Task<IActionResult> Outstanding([FromRoute]string hashedAccountId)
        {
            var response = await _mediator.Send(new GetLatestPendingReceivedTransferConnectionInvitationQuery
            {
                AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId)
            });

            return response.TransferConnectionInvitation == null
                ? RedirectToAction("Index", "TransferConnections")
                : RedirectToAction("Receive", new { transferConnectionInvitationId = response.TransferConnectionInvitation.Id, hashedAccountId });
        }
    }
}