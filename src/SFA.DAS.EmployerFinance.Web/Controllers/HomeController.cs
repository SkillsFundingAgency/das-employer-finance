using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.Infrastructure;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [Route("service")]
    public class HomeController : Controller
    { 
        private readonly ZenDeskConfiguration _configuration;
        private readonly IUrlActionHelper _urlHelper;
        private readonly IStubAuthenticationService _stubAuthenticationService;
        private readonly IConfiguration _config;

        public HomeController(ZenDeskConfiguration configuration, IUrlActionHelper urlHelper, IStubAuthenticationService stubAuthenticationService, IConfiguration config)
        {
            _configuration = configuration;
            _urlHelper = urlHelper;
            _stubAuthenticationService = stubAuthenticationService;
            _config = config;
        }

        public IActionResult Index()
        {
            return Redirect(_urlHelper.LegacyEasAction(string.Empty));
        }

        [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
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

        [Route("signOut", Name = RouteNames.SignOut)]
        public async Task<IActionResult> SignOutUser()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token", idToken);
            var schemes = new List<string>
            {
                CookieAuthenticationDefaults.AuthenticationScheme
            };
            _ = bool.TryParse(_config["StubAuth"], out var stubAuth);
            if (!stubAuth)
            {
                schemes.Add(OpenIdConnectDefaults.AuthenticationScheme);
            }
            
            return SignOut(authenticationProperties, schemes.ToArray());
        }

        [Route("SignOutCleanup")]
        public async Task SignOutCleanup()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token", idToken);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, authenticationProperties);
        }

        [HttpGet]
        [Route("help")]
        public IActionResult Help()
        {
            return RedirectPermanent(_configuration.ZenDeskHelpCentreUrl);
        }

        [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
        [Route("signIn")]
        public IActionResult SignIn()
        {
            return RedirectToAction(ControllerConstants.IndexActionName);
        }

#if DEBUG
        [HttpGet]
        [Route("SignIn-Stub")]
        public IActionResult SigninStub()
        {
            return View("SigninStub", new List<string>{_config["StubId"],_config["StubEmail"]});
        }
        [HttpPost]
        [Route("SignIn-Stub")]
        public async Task<IActionResult> SigninStubPost()
        {
            
            var claims = await _stubAuthenticationService.GetStubSignInClaims(new StubAuthUserDetails
            {
                Email = _config["StubEmail"],
                Id = _config["StubId"]
            });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
                new AuthenticationProperties());
            
            return RedirectToRoute("Signed-in-stub");
        }

        [Authorize]
        [HttpGet]
        [Route("signed-in-stub", Name = "Signed-in-stub")]
        public IActionResult SignedInStub()
        {
            return View();
        }
#endif
    }
}