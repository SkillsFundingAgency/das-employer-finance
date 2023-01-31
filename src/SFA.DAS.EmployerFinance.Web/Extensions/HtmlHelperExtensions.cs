using System;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediatR;
using SFA.DAS.Authorization.Services;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Helpers;
using SFA.DAS.EmployerFinance.Queries.GetContent;
using SFA.DAS.MA.Shared.UI.Configuration;
using SFA.DAS.MA.Shared.UI.Models;
using SFA.DAS.MA.Shared.UI.Models.Links;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using SFA.DAS.EmployerFinance.Web.Helpers;

namespace SFA.DAS.EmployerFinance.Web.Extensions
{
    public interface IHtmlHelperExtensions
    {
        HtmlString CdnLink(string folderName, string fileName);
        HtmlString SetZenDeskLabels(params string[] zenDeskLabels);
        string EscapeApostrophes(string input);
        string GetZenDeskSnippetKey();
        string GetZenDeskSnippetSectionId();
        string GetZenDeskCobrowsingSnippetKey();
        IHeaderViewModel GetHeaderViewModel(IHtmlHelper html, bool useLegacyStyles = false);
        IFooterViewModel GetFooterViewModel(IHtmlHelper html, bool useLegacyStyles = false);
        ICookieBannerViewModel GetCookieBannerViewModel(IHtmlHelper html);
        HtmlString GetContentByType(string type, bool useLegacyStyles = false);
        bool IsAuthorized(string featureType);
    }
    public class HtmlHelperExtensions : IHtmlHelperExtensions
    {
        private readonly EmployerFinanceConfiguration _configuration;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HtmlHelperExtensions> _logger;
        private readonly IAuthorizationService _authorisationService;
        private readonly ICompositeViewEngine _compositeViewEngine;

        public HtmlHelperExtensions(
            EmployerFinanceConfiguration configuration,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor,
            ILogger<HtmlHelperExtensions> logger,
            IAuthorizationService authorisationService,
            ICompositeViewEngine compositeViewEngine)
        {
            _configuration = configuration;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _authorisationService = authorisationService;
            _compositeViewEngine = compositeViewEngine;
        }
        public HtmlString CdnLink(string folderName, string fileName)
        {
            var cdnLocation = _configuration.CdnBaseUrl;

            var trimCharacters = new char[] { '/' };
            return new HtmlString($"{cdnLocation.Trim(trimCharacters)}/{folderName.Trim(trimCharacters)}/{fileName.Trim(trimCharacters)}");
        }

        public HtmlString SetZenDeskLabels(this HtmlHelper html, params string[] labels)
        {
            var keywords = string.Join(",", labels
               .Where(label => !string.IsNullOrEmpty(label))
               .Select(label => $"'{EscapeApostrophes(label)}'"));

            // when there are no keywords default to empty string to prevent zen desk matching articles from the url
            var apiCallString = "<script type=\"text/javascript\">zE('webWidget', 'helpCenter:setSuggestions', { labels: ["
                + (!string.IsNullOrEmpty(keywords) ? keywords : "''")
                + "] });</script>";

            return new HtmlString(apiCallString);
        }

        private static string EscapeApostrophes(string input)
        {
            return input.Replace("'", @"\'");
        }

        public string GetZenDeskSnippetKey()
        {     
            return _configuration.ZenDeskSnippetKey;
        }

        public string GetZenDeskSnippetSectionId()
        {
            return _configuration.ZenDeskSectionId;
        }

        public string GetZenDeskCobrowsingSnippetKey()
        {
            return _configuration.ZenDeskCobrowsingSnippetKey;
        }

        public IHeaderViewModel GetHeaderViewModel(IHtmlHelper html, bool useLegacyStyles = false)
        {
            var employerFinanceBaseUrl = _configuration.EmployerFinanceBaseUrl + (_configuration.EmployerFinanceBaseUrl.EndsWith("/") ? "" : "/");

            var headerModel = new HeaderViewModel(new HeaderConfiguration
            {
                ManageApprenticeshipsBaseUrl = _configuration.EmployerAccountsBaseUrl,
                ApplicationBaseUrl = _configuration.EmployerAccountsBaseUrl,
                EmployerCommitmentsBaseUrl = _configuration.EmployerCommitmentsBaseUrl,
                EmployerCommitmentsV2BaseUrl = _configuration.EmployerCommitmentsV2BaseUrl,
                EmployerFinanceBaseUrl = _configuration.EmployerFinanceBaseUrl,
                AuthenticationAuthorityUrl = _configuration.Identity.BaseAddress,
                ClientId = _configuration.Identity.ClientId,
                EmployerRecruitBaseUrl = _configuration.EmployerRecruitBaseUrl,
                SignOutUrl = new Uri($"{employerFinanceBaseUrl}service/signOut"),
                ChangeEmailReturnUrl = new Uri($"{employerFinanceBaseUrl}service/email/change"),
                ChangePasswordReturnUrl = new Uri($"{employerFinanceBaseUrl}service/password/change")
            },
            new UserContext
            {
                User = html.ViewContext.HttpContext.User,
                HashedAccountId = html.ViewContext.RouteData.Values["HashedAccountId"]?.ToString()
            },
            useLegacyStyles: useLegacyStyles
            );

            headerModel.SelectMenu(html.ViewContext.RouteData.Values["Controller"].ToString() == "EmployerCommitments" ? "EmployerCommitments" : html.ViewBag.Section);

            if (html.ViewBag.HideNav != null && html.ViewBag.HideNav)
            {
                headerModel.HideMenu();
            }

            if (html.ViewData.Model?.GetType().GetProperty("HideHeaderSignInLink") != null)
            {
                headerModel.RemoveLink<SignIn>();
            }

            return headerModel;
        }

        public IFooterViewModel GetFooterViewModel(HtmlHelper html, bool useLegacyStyles = false)
        {
            return new FooterViewModel(new FooterConfiguration
            {
                ManageApprenticeshipsBaseUrl = _configuration.EmployerAccountsBaseUrl
            },
            new UserContext
            {
                User = html.ViewContext.HttpContext.User,
                HashedAccountId = html.ViewContext.RouteData.Values["HashedAccountId"]?.ToString()
            },
            useLegacyStyles: useLegacyStyles
            );
        }

        public ICookieBannerViewModel GetCookieBannerViewModel(HtmlHelper html)
        {
            return new CookieBannerViewModel(new CookieBannerConfiguration
            {
                ManageApprenticeshipsBaseUrl = _configuration.EmployerAccountsBaseUrl
            },
            new UserContext
            {
                User = html.ViewContext.HttpContext.User,
                HashedAccountId = html.ViewContext.RouteData.Values["accountHashedId"]?.ToString()
            }
            );
        }

        public HtmlString GetContentByType(string type, bool useLegacyStyles = false)
        {
            var response = AsyncHelper.RunSync(() => _mediator.Send(new GetContentRequest
            {
                UseLegacyStyles = useLegacyStyles,
                ContentType = type
            }));

            return new HtmlString(response.Content);
        }

        public static bool IsAuthorized(string featureType)
        {
            var isAuthorized = authorisationService.IsAuthorized(featureType);

            return isAuthorized;
        }
    }
}   