using System.Text.Json;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerUsers.WebClientComponents;

namespace SFA.DAS.EmployerFinance.Web.Controllers;

public class BaseController : Controller
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IMediator Mediator;
    
    private readonly ILogger<TransferConnectionsController> _logger;

    public BaseController(IHttpContextAccessor httpContextAccessor, IMediator mediator, ILogger<TransferConnectionsController> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        Mediator = mediator;
        _logger = logger;
    }

    protected async Task UpsertRegisteredUser()
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
        
        _logger.LogInformation("{TypeName}.UpsertRegisteredUser() called: '{Command}'.", nameof(BaseController), JsonSerializer.Serialize(command));
        
        await Mediator.Send(command);
    }
}