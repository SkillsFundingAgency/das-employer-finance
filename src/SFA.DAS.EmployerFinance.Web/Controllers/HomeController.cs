using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerFinance.Web.Helpers;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [Route("service")]
    public class HomeController : Controller
    {
        private readonly IAuthenticationService _owinWrapper;
        private readonly EmployerFinanceConfiguration _configuration;

        public HomeController(IAuthenticationService owinWrapper, EmployerFinanceConfiguration configuration)
        {
            _owinWrapper = owinWrapper;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return Redirect(Url.LegacyEasAction(string.Empty));
        }

        [DasAuthorize]
        public IActionResult SignedIn()
        {
            return View();
        }

        [HttpGet]
        [Route("{HashedAccountId}/privacy", Order = 0)]
        [Route("privacy", Order = 1)]
        public IActionResult Privacy()
        {
            return Redirect(Url.EmployerAccountsAction("service", "privacy"));
        }

        [HttpGet]
        [Route("{HashedAccountId}/cookieConsent", Order = 0)]
        [Route("cookieConsent", Order = 1)]
        public IActionResult CookieConsent()
        {
            return Redirect(Url.EmployerAccountsAction("cookieConsent"));
        }       

        [Route("signOut")]
        public IActionResult SignOut()
        {
            _owinWrapper.SignOutUser();

            var owinContext = HttpContext.GetOwinContext();
            var authenticationManager = owinContext.Authentication;
            var idToken = authenticationManager.User.FindFirst("id_token")?.Value;
            var constants = new Constants(_configuration.Identity);

            return new RedirectResult(string.Format(constants.LogoutEndpoint(), idToken));
        }

        [Route("SignOutCleanup")]
        public void SignOutCleanup()
        {
            _owinWrapper.SignOutUser();
        }

        [HttpGet]
        [Route("help")]
        public IActionResult Help()
        {
            return RedirectPermanent(_configuration.ZenDeskHelpCentreUrl);
        }

        [DasAuthorize]
        [Route("signIn")]
        public IActionResult SignIn()
        {
            return RedirectToAction(ControllerConstants.IndexActionName);
        }       
    }
}