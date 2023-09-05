using SFA.DAS.Authentication;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.EmployerUsers.WebClientComponents;
using IAuthenticationService = SFA.DAS.Authentication.IAuthenticationService;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    public class BaseController : Controller
    {
        private const string FlashMessageCookieName = "sfa-das-employerapprenticeshipsservice-flashmessage";

        private readonly ICookieStorageService<FlashMessageViewModel> _flashMessage;
        protected IAuthenticationService OwinWrapper;
        protected IMediator Mediator;

        public BaseController(ICookieStorageService<FlashMessageViewModel> flashMessage, IAuthenticationService owinWrapper, IMediator mediator)
        {
            _flashMessage = flashMessage;
            OwinWrapper = owinWrapper;
            Mediator = mediator;
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
            var userRef = OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName);
            var email = OwinWrapper.GetClaimValue(ControllerConstants.EmailClaimKeyName);
            var firstName = OwinWrapper.GetClaimValue(DasClaimTypes.GivenName);
            var lastName = OwinWrapper.GetClaimValue(DasClaimTypes.FamilyName);

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