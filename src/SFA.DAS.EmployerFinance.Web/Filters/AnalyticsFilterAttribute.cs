using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.UserAccounts;
using SFA.DAS.EmployerFinance.Web.Extensions;

namespace SFA.DAS.EmployerFinance.Web.Filters;

public class AnalyticsFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string hashedAccountId = null;
        string levyFlag = null;

        var thisController = context.Controller as Microsoft.AspNetCore.Mvc.Controller;
        if (thisController != null)
        {
            var user= thisController.User;
            var userId = user?.GetUserId();

            if (thisController.RouteData.Values.TryGetValue(RouteValues.EncodedAccountId, out var accountHashedId))
            {
                hashedAccountId = accountHashedId.ToString().ToUpper();

                var accountsJson = thisController.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier))?.Value;
                if (accountsJson is not null)
                {
                    var accounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(accountsJson);
                    levyFlag = accounts.TryGetValue(hashedAccountId, out var employer) ? employer.ApprenticeshipEmployerType.ToString() : null;
                }
            }

            thisController.ViewBag.GaData = new GaData
            {
                UserId = userId,
                Acc = hashedAccountId,
                LevyFlag = levyFlag
            };
        }
        base.OnActionExecuting(context);
    }

    public string DataLoaded { get; set; }

    public class GaData
    {
        public string DataLoaded { get; set; } = "dataLoaded";
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string Vpv { get; set; }
        public string Acc { get; set; }
        public string LevyFlag { get; set; }
        public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
    }
}