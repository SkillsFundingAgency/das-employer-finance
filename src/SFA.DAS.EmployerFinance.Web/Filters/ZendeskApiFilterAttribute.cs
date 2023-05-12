using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerFinance.Web.Authorization;
using SFA.DAS.EmployerFinance.Web.Extensions;

namespace SFA.DAS.EmployerFinance.Web.Filters;

public class ZendeskApiFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var controller = filterContext.Controller as Controller;
        if (controller != null)
        {
            var accountIdFromUrl = controller.RouteData.Values[RouteValueKeys.HashedAccountId]?.ToString().ToUpper();

            controller.ViewBag.ZendeskApiData = new ZendeskApiData
            {
                Name = controller.User.GetDisplayName(),
                Email = controller.User.GetEmailAddress(),
                Organization = accountIdFromUrl
            };
        }

        base.OnActionExecuting(filterContext);
    }

    public class ZendeskApiData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
    }
}