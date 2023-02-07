using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.Validation.Mvc;

namespace SFA.DAS.EmployerFinance.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ImportModelStateFromTempDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            //TODO MAP-192 Need implementing
            //ModelStateDictionary dictionary = (controller.TempData["ModelState"] as SerializableModelStateDictionary)?.ToModelState();
            //controller.ViewData.ModelState.Merge(dictionary);

            //base.OnActionExecuted(filterContext);
        }
    }
}