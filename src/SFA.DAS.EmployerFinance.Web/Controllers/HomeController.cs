using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

        [Route("signOut")]
        public async Task<IActionResult> SignOutUser()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token", idToken);
            return SignOut(authenticationProperties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
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
        public IActionResult SigninStubPost()
        {
            _stubAuthenticationService?.AddStubEmployerAuth(Response.Cookies, new StubAuthUserDetails
            {
                Email = _config["StubEmail"],
                Id = _config["StubId"]
            });

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