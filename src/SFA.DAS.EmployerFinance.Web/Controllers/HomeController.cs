using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Helpers;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [Route("service")]
    public class HomeController : Controller
    { 
        private readonly EmployerFinanceConfiguration _configuration;
        private readonly IUrlActionHelper _urlHelper;

        public HomeController(EmployerFinanceConfiguration configuration, IUrlActionHelper urlHelper)
        {
            _configuration = configuration;
            _urlHelper = urlHelper;
        }

        public IActionResult Index()
        {
            return Redirect(_urlHelper.LegacyEasAction(string.Empty));
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
            //return Redirect(_urlHelper.EmployerAccountsAction("service", "privacy"));
            return View();
        }

        [HttpGet]
        [Route("{HashedAccountId}/cookieConsent", Order = 0)]
        [Route("cookieConsent", Order = 1)]
        public IActionResult CookieConsent()
        {
            return Redirect(_urlHelper.EmployerAccountsAction("cookieConsent"));
        }

        [Route("signOut")]
        public async Task<IActionResult> SignOut()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token", idToken);
            SignOut(authenticationProperties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

            var constants = new Constants(_configuration.Identity);

            return new RedirectResult(string.Format(constants.LogoutEndpoint(), idToken));
        }

        [Route("SignOutCleanup")]
        public async void SignOutCleanup()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token", idToken);

            SignOut(authenticationProperties,CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme) ;
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