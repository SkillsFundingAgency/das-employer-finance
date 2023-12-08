using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
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

        var userRef = userIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = userIdentity.Claims.FirstOrDefault(c => c.Type == DasClaimTypes.Email)?.Value;
        var firstName = userIdentity.Claims.FirstOrDefault(c => c.Type == DasClaimTypes.GivenName)?.Value;
        var lastName = userIdentity.Claims.FirstOrDefault(c => c.Type == DasClaimTypes.FamilyName)?.Value;
        
        _logger.LogInformation("{TypeName}.UpsertRegisteredUser() called with FirstName = '{FirstName}', LastName = '{LastName}'.", nameof(BaseController), firstName, lastName);

        await Mediator.Send(new UpsertRegisteredUserCommand
        {
            EmailAddress = email,
            UserRef = userRef,
            LastName = lastName,
            FirstName = firstName
        });
    }
}