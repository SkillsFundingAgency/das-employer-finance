using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerUsers.WebClientComponents;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    public class BaseController : Controller
    {
        protected IHttpContextAccessor HttpContextAccessor;
        protected IMediator Mediator;

        public BaseController(IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            HttpContextAccessor = httpContextAccessor;
            Mediator = mediator;
        }

       public BaseController() { }

        protected async Task UpsertRegisteredUser()
        {
            var userIdentity = HttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;

            var userRef = userIdentity.Claims.FirstOrDefault(c => c.Type == ControllerConstants.UserRefClaimKeyName)?.Value;
            var email = userIdentity.Claims.FirstOrDefault(c => c.Type == DasClaimTypes.Email)?.Value;
            var firstName = userIdentity.Claims.FirstOrDefault(c => c.Type == DasClaimTypes.GivenName)?.Value;
            var lastName = userIdentity.Claims.FirstOrDefault(c => c.Type == DasClaimTypes.FamilyName)?.Value;

            await Mediator.Send(new UpsertRegisteredUserCommand
            {
                EmailAddress = email,
                UserRef = userRef,
                LastName = lastName,
                FirstName = firstName
            });
        }
    }
}