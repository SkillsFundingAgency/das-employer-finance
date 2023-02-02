using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.EmployerFinance.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HttpNotFoundForNullModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            //MAP-192 Need implementing
            //if (controller.Response != null && !controller.Response.TryGetContentValue<object>(out var _))
            //{
            //    controller.Response = controller.Request.CreateResponse(HttpStatusCode.NotFound);
            //}
        }
    }
}