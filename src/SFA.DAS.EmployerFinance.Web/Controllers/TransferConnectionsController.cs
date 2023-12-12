using System.Text.Json;
using AutoMapper;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Attributes;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerFinance.Web.Infrastructure;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.Controllers;

[SetNavigationSection(NavigationSection.AccountsFinance)]
[Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
[Route("accounts/{HashedAccountId}/transfers/connections")]
public class TransferConnectionsController : Controller
{
    private readonly ILogger<TransferConnectionsController> _logger;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IEncodingService _encodingService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TransferConnectionsController(ILogger<TransferConnectionsController> logger,
        IMapper mapper,
        IMediator mediator,
        IEncodingService encodingService,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _mapper = mapper;
        _mediator = mediator;
        _encodingService = encodingService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    [Route("",Name = RouteNames.TransferConnectionsIndex)]
    public async Task<IActionResult> Index([FromRoute]string hashedAccountId)
    {
        await UpsertRegisteredUser();

        var response = await _mediator.Send(new GetEmployerAccountDetailByHashedIdQuery
        {
            HashedAccountId = hashedAccountId
        });

        var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        //This cant be done with Task.WhenAll
        var transferAllowance = await TransferAllowance(accountId);
        var transferConnectionInvitationAuthorization = await TransferConnectionInvitationAuthorization(accountId);
        var transferConnectionInvitations =  await TransferConnectionInvitations(accountId);
        var transferRequests = await TransferRequests(accountId);

        transferConnectionInvitationAuthorization.HashedAccountId = hashedAccountId;
            
        var model = new TransferViewModel
        {
            ApprenticeshipEmployerType = response.AccountDetail.ApprenticeshipEmployerType,
            TransferAllowanceViewModel = transferAllowance,
            TransferConnectionInvitationAuthorizationViewModel = transferConnectionInvitationAuthorization,
            TransferConnectionInvitationsViewModel = transferConnectionInvitations,
            TransferRequest = transferRequests,
        };
            
        return View(model);
    }
        
    [NonAction]
    public async Task<TransferAllowanceViewModel> TransferAllowance(long accountId)
    {
        var response = await _mediator.Send(new GetTransferAllowanceQuery
        {
            AccountId = accountId
        });
        return _mapper.Map<TransferAllowanceViewModel>(response);
    }
        
    [NonAction]
    public async Task<TransferConnectionInvitationAuthorizationViewModel> TransferConnectionInvitationAuthorization(long accountId)
    {
        var response = await _mediator.Send(new GetTransferConnectionInvitationAuthorizationQuery
        {
            AccountId = accountId
        });
        return _mapper.Map<TransferConnectionInvitationAuthorizationViewModel>(response);
    }

    [NonAction]
    public async Task<TransferConnectionInvitationsViewModel> TransferConnectionInvitations(long accountId)
    {
        var response = await _mediator.Send(new GetTransferConnectionInvitationsQuery
        {
            AccountId = accountId
        });

        return new TransferConnectionInvitationsViewModel
        {
            AccountId = response.AccountId,
            HashedAccountId = _encodingService.Encode(response.AccountId, EncodingType.AccountId),
            TransferSenderConnectionInvitations = response.TransferConnectionInvitations
                .Where(p => p.SenderAccount.Id.Equals(response.AccountId)).Select(c =>
                    new TransferConnectionInvitationDto
                    {
                        Changes = c.Changes,
                        Id = c.Id,
                        Status = c.Status,
                        CreatedDate = c.CreatedDate,
                        ReceiverAccount = c.ReceiverAccount,
                        SenderAccount = c.SenderAccount,
                        HashedId = _encodingService.Encode(c.Id, EncodingType.TransferRequestId)
                    }),
            TransferReceiverConnectionInvitations = response.TransferConnectionInvitations
                .Where(p => p.ReceiverAccount.Id.Equals(response.AccountId)).Select(c =>
                    new TransferConnectionInvitationDto
                    {
                        Changes = c.Changes,
                        Id = c.Id,
                        Status = c.Status,
                        CreatedDate = c.CreatedDate,
                        ReceiverAccount = c.ReceiverAccount,
                        SenderAccount = c.SenderAccount,
                        HashedId = _encodingService.Encode(c.Id, EncodingType.TransferRequestId)
                    })
        };
    }

    [NonAction]
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
        
    private async Task UpsertRegisteredUser()
    {
        var userIdentity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;

        var email = userIdentity.GetValueFor(DasClaimTypes.Email);
        var firstName = userIdentity.GetValueFor(DasClaimTypes.GivenName);
        var lastName = userIdentity.GetValueFor(DasClaimTypes.FamilyName);
        var userRef = userIdentity.GetValueFor(EmployerClaims.IdamsUserIdClaimTypeIdentifier);
        
        var command = new UpsertRegisteredUserCommand
        {
            EmailAddress = email,
            UserRef = userRef,
            LastName = lastName,
            FirstName = firstName
        };
        
        _logger.LogInformation("UpsertRegisteredUser() called. Command properties: {Command}", JsonSerializer.Serialize(command));
        
        await _mediator.Send(command);
    }
}