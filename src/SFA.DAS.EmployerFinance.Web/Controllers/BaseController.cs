using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.EmployerUsers.WebClientComponents;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    public class BaseController : Controller
    {
        private const string FlashMessageCookieName = "sfa-das-employerapprenticeshipsservice-flashmessage";

        private readonly ICookieStorageService<FlashMessageViewModel> _flashMessage;
        protected IHttpContextAccessor HttpContextAccessor;
        protected IMediator Mediator;
        private readonly ILogger<BaseController> _logger;

        public BaseController(ICookieStorageService<FlashMessageViewModel> flashMessage, IHttpContextAccessor httpContextAccessor, IMediator mediator, ILogger<BaseController> logger)
        {
            _flashMessage = flashMessage;
            HttpContextAccessor = httpContextAccessor;
            Mediator = mediator;
            _logger = logger;
        }

       public BaseController() { }

        public void AddFlashMessageToCookie(FlashMessageViewModel model)
        {
            _flashMessage.Delete(FlashMessageCookieName);

            _flashMessage.Create(model, FlashMessageCookieName);
        }

        public FlashMessageViewModel GetFlashMessageViewModelFromCookie()
        {
            var flashMessageViewModelFromCookie = _flashMessage.Get(FlashMessageCookieName);
            _flashMessage.Delete(FlashMessageCookieName);
            return flashMessageViewModelFromCookie;
        }

        protected async Task UpsertRegisteredUser()
        {
            var userIdentity = HttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;

            _logger.LogInformation("Listing claims for user...");
            foreach (var claim in userIdentity.Claims)
            {
                _logger.LogInformation($"{claim.Type}: {claim.Value}");
            }

            var userRef = userIdentity.Claims.FirstOrDefault(c => c.Type == ControllerConstants.UserRefClaimKeyName)?.Value;
            var email = userIdentity.Claims.FirstOrDefault(c => c.Type == ControllerConstants.EmailClaimKeyName)?.Value;
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