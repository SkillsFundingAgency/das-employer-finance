using System;
using System.ComponentModel.DataAnnotations;
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
        [Route("start")]
        public async Task<IActionResult> Start([FromRoute]string hashedAccountId, StartTransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Start", model);
            }

            try
            {
                await _mediator.Send(new SendTransferConnectionInvitationQuery
                {
                    ReceiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId,
                    AccountId = _encodingService.Decode(model.HashedAccountId, EncodingType.AccountId)
                });
                return RedirectToAction("Send", new {hashedAccountId, receiverAccountPublicHashedId = model.ReceiverAccountPublicHashedId });
            }
            catch (ValidationException e)
            {
                foreach (var member in e.ValidationResult.MemberNames)
                {
                    ModelState.AddModelError(member.Split('|')[0], member.Split('|')[1]);
                }
                return View("Start", model);
            }
            
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

            model.HashedAccountId = hashedAccountId;
            model.HashedTransferConnectionInvitationId = transferConnectionInvitationId;
            
            return View(model);
        }

        [HttpPost]
        [Route("{transferConnectionInvitationId}/sent")]
        public IActionResult Sent([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId, SentTransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Sent", model);
            }
            
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

        [Route("{transferConnectionInvitationId}/receive")]
        public async Task<IActionResult> Receive([FromRoute]string hashedAccountId, [FromRoute]string transferConnectionInvitationId)
        {
            var response = await _mediator.Send(new GetReceivedTransferConnectionInvitationQuery
            {
                AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId)
            });
            var model = _mapper.Map<ReceiveTransferConnectionInvitationViewModel>(response);
            model.HashedAccountId = hashedAccountId;
            model.NotHashedTransferConnectionInvitationId = Convert.ToInt32(_encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId));
            model.TransferConnectionInvitationId = transferConnectionInvitationId;
            return View(model);
        }

        [HttpPost]
        [Route("{transferConnectionInvitationId}/receive")]
        public async Task<IActionResult> Receive([FromRoute]string hashedAccountId, [FromRoute]string transferConnectionInvitationId, ReceiveTransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var response = await _mediator.Send(new GetReceivedTransferConnectionInvitationQuery
                {
                    AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                    TransferConnectionInvitationId = _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId)
                });
                model = _mapper.Map<ReceiveTransferConnectionInvitationViewModel>(response);
                model.HashedAccountId = hashedAccountId;
                model.NotHashedTransferConnectionInvitationId = Convert.ToInt32(_encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId));
                model.TransferConnectionInvitationId = transferConnectionInvitationId;
                return View("Receive", model);
            }
            
            switch (model.Choice)
            {
                case "Approve":
                    await _mediator.Send(new ApproveTransferConnectionInvitationCommand
                    {
                        AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId), 
                        UserRef = Guid.Parse(User.GetUserId()), 
                        TransferConnectionInvitationId = Convert.ToInt32(_encodingService.Decode(model.TransferConnectionInvitationId, EncodingType.TransferRequestId))
                    });
                    return RedirectToAction("Approved", new { transferConnectionInvitationId, hashedAccountId });
                case "Reject":
                    await _mediator.Send(new RejectTransferConnectionInvitationCommand
                    {
                        AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId), 
                        UserRef = Guid.Parse(User.GetUserId()), 
                        TransferConnectionInvitationId = Convert.ToInt32(_encodingService.Decode(model.TransferConnectionInvitationId, EncodingType.TransferRequestId))
                    });
                    return RedirectToAction("Rejected", new { transferConnectionInvitationId, hashedAccountId });
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
            model.HashedAccountId = hashedAccountId;
            model.HashedTransferConnectionInvitationId = transferConnectionInvitationId;
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
            model.HashedAccountId = hashedAccountId;
            model.HashedTransferConnectionInvitationId = transferConnectionInvitationId;
            return View(model);
        }

        [HttpPost]
        [ValidateModelState]
        [Route("{transferConnectionInvitationId}/rejected")]
        public async Task<IActionResult> Rejected([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId, RejectedTransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Rejected", model);
            }
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
                    return RedirectToAction("Index", "TransferConnections", new {hashedAccountId});
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }

        [Route("{transferConnectionInvitationId}/details")]
        public async Task<IActionResult> Details([FromRoute]string hashedAccountId, [FromRoute]string transferConnectionInvitationId)
        {
            var model = await GetTransferConnectionInvitationViewModel(hashedAccountId, transferConnectionInvitationId);
            return View(model);
        }

        [HttpPost]
        [Route("{transferConnectionInvitationId}/details")]
        public async Task<IActionResult> Details([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId, TransferConnectionInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await GetTransferConnectionInvitationViewModel(hashedAccountId, transferConnectionInvitationId);
                return View("Details", model);
            }
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
                    return RedirectToAction("Index", "TransferConnections", new {hashedAccountId});
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Choice));
            }
        }
        
        private async Task<TransferConnectionInvitationViewModel> GetTransferConnectionInvitationViewModel(string hashedAccountId,
            string transferConnectionInvitationId)
        {
            var response = await _mediator.Send(new GetTransferConnectionInvitationQuery
            {
                AccountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId),
                TransferConnectionInvitationId =
                    _encodingService.Decode(transferConnectionInvitationId, EncodingType.TransferRequestId)
            });
            var model = _mapper.Map<TransferConnectionInvitationViewModel>(response);
            model.HashedAccountId = hashedAccountId;
            model.HashedTransferConnectionInvitationId = transferConnectionInvitationId;
            model.TransferConnectionInvitation.ReceiverAccount.PublicHashedId =
                _encodingService.Encode(model.TransferConnectionInvitation.ReceiverAccount.Id, EncodingType.PublicAccountId);
            model.TransferConnectionInvitation.SenderAccount.PublicHashedId =
                _encodingService.Encode(model.TransferConnectionInvitation.SenderAccount.Id, EncodingType.PublicAccountId);
            return model;
        }

        [ImportModelStateFromTempData]
        [Route("{transferConnectionInvitationId}/deleted")]
        public IActionResult Deleted([FromRoute]string hashedAccountId, [FromRoute]string transferConnectionInvitationId)
        {
            var model = new DeletedTransferConnectionInvitationViewModel
            {
                HashedAccountId = hashedAccountId,
                HashedTransferConnectionInvitationId = transferConnectionInvitationId
            };

            return View(model);
        }

        [HttpPost]
        [Route("{transferConnectionInvitationId}/deleted")]
        public IActionResult Deleted([FromRoute]string hashedAccountId,[FromRoute]string transferConnectionInvitationId, DeletedTransferConnectionInvitationViewModel model)
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