using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.EmployerFinance.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

            //filterContext.Controller.TempData["ModelState"] = filterContext.Controller.ViewData.ModelState.ToSerializable();
            //filterContext.RouteData.Values.Merge(filterContext.HttpContext.Request.QueryString);
            //filterContext.Result = new RedirectToRouteResult(filterContext.RouteData.Values);

            var controller = filterContext.Controller as Controller;
            //MAP-192 Need implementing
            //ValidationException ex;
            //if((ex=filterContext.Exception as ValidationException) != null)
            //{
            //    controller.ViewData.ModelState.AddModelError("",ex, );
            //    controller.TempData["ModelState"] = controller.ViewData.ModelState.ToArray();
            //    controller.RouteData.Values.Add(filterContext.HttpContext.Request.QueryString);
            //    filterContext.Result = new RedirectToRouteResult(filterContext.RouteData.Values);
            //    filterContext.ExceptionHandled = true;
            //}

            base.OnActionExecuted(filterContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            //MAP-192 Need implementing
            //if (!controller.ViewData.ModelState.IsValid)
            //{
            //    controller.TempData["ModelState"] = controller.ViewData.ModelState.ToArray();
            //    controller.RouteData.Values.Add(filterContext.HttpContext.Request.QueryString);
            //    filterContext.Result = new RedirectToRouteResult(filterContext.RouteData.Values);
            //}

            //base.OnActionExecuting(filterContext);
        }
    }
}