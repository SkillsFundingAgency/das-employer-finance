﻿using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerFinance.Web.Infrastructure;

namespace SFA.DAS.EmployerFinance.Web.Filters;

public class AnalyticsFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var thisController = context.Controller as Microsoft.AspNetCore.Mvc.Controller;
        if (thisController != null)
        {
            var user= thisController.User;
            var userId = user?.GetUserId();
            thisController.ViewBag.GaData = new GaData
            {
                UserId = userId,
                Acc = thisController.RouteData.Values[RouteValues.EncodedAccountId]?.ToString().ToUpper()
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

        public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
    }
}