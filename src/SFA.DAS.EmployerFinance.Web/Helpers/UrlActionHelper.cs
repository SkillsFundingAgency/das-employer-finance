using Microsoft.AspNetCore.Mvc.Infrastructure;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Web.Helpers
{
    public class UrlActionHelper : IUrlActionHelper
    {
        private readonly EmployerFinanceWebConfiguration _configuration;
        private readonly IActionContextAccessor _actionContextAccessor;

        public UrlActionHelper(EmployerFinanceWebConfiguration configuration, IActionContextAccessor actionContextAccessor)
        {
            _configuration = configuration;
            _actionContextAccessor = actionContextAccessor;
        }

        public string EmployerAccountsAction(string path, bool withAccountContext = true)
        {
            var baseUrl = _configuration.EmployerAccountsBaseUrl;

            return AccountAction(baseUrl, path, withAccountContext);
        }

        public string EmployerCommitmentsV2Action(string path)
        {
            var baseUrl = _configuration.EmployerCommitmentsV2BaseUrl;

            return NonAccountsAction(baseUrl, path);
        }

        public string LevyTransfersMatchingAccountAction(string path, bool withAccountContext = true)
        {
            var baseUrl = _configuration.LevyTransferMatchingBaseUrl;

            return AccountAction(baseUrl, path, withAccountContext);
        }

        public string EmployerFinanceAction(string path)
        {
            var baseUrl = _configuration.EmployerFinanceBaseUrl;

            return AccountAction(baseUrl, path);
        }

        public string EmployerProjectionsAction(string path)
        {
            var baseUrl = _configuration.EmployerProjectionsBaseUrl;

            return AccountAction(baseUrl, path);
        }

        
        public string LegacyEasAction(string path)
        {
            var baseUrl = _configuration.EmployerPortalBaseUrl;

            return Action(baseUrl, path);
        }

        private string AccountAction(string baseUrl, string path, bool withAccountContext = true)
        {
            string accountPath = string.Empty;

            if (withAccountContext)
            {
                var hashedAccountId = _actionContextAccessor.ActionContext.RouteData.Values[ControllerConstants.AccountHashedIdRouteKeyName];
                accountPath = hashedAccountId == null ? $"accounts/{path}" : $"accounts/{hashedAccountId}/{path}";
            }

            path = string.IsNullOrEmpty(accountPath) ? path : accountPath;

            return Action(baseUrl, accountPath);
        }

        // unlike the rest of the services within MA - commitments v2 does not have 'accounts/' in its urls
        // Nor does Employer Feedback
        private string NonAccountsAction(string baseUrl, string path)
        {

            var hashedAccountId = _actionContextAccessor.ActionContext.RouteData.Values[ControllerConstants.AccountHashedIdRouteKeyName];
            var commitmentPath = hashedAccountId == null ? $"{path}" : $"{hashedAccountId}/{path}";

            return Action(baseUrl, commitmentPath);
        }


        private static string Action(string baseUrl, string path)
        {
            var trimmedBaseUrl = baseUrl?.TrimEnd('/') ?? string.Empty;

            return $"{trimmedBaseUrl}/{path}".TrimEnd('/');
        }
    }
}
