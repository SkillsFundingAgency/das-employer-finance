using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerFinance.Web.Filters
{
    public class LevyEmployerTypeOnly : Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute
    {

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext filterContext)
        {
            try
            {
                string hashedAccountId = string.Empty;

                var controller = filterContext.Controller as Controller;

                if (controller != null && controller.RouteData.Values.ContainsKey("HashedAccountId"))
                {
                    hashedAccountId = controller.RouteData.Values[RouteValues.EncodedAccountId].ToString();

                }
                else if (controller.RouteData?.Values?.ContainsKey(RouteValues.EncodedAccountId) == true)
                {

                    hashedAccountId = controller.RouteData.Values[RouteValues.EncodedAccountId].ToString();
                }


                if (string.IsNullOrWhiteSpace(hashedAccountId))
                {
                    filterContext.Result = new Microsoft.AspNetCore.Mvc.ViewResult { ViewName = ControllerConstants.BadRequestViewName };
                    return;
                }

                //MAP-192 - need testing
                var accountApi = filterContext.HttpContext.RequestServices.GetService<IAccountApiClient>();

                var task = Task.Run(async () => await accountApi.GetAccount(hashedAccountId));
                AccountDetailViewModel account = task.Result;
                ApprenticeshipEmployerType apprenticeshipEmployerType = (ApprenticeshipEmployerType)Enum.Parse(typeof(ApprenticeshipEmployerType), account.ApprenticeshipEmployerType, true);

                if (apprenticeshipEmployerType == ApprenticeshipEmployerType.Levy)
                {
                    base.OnActionExecuting(filterContext);
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(
                            new
                            {
                                controller = ControllerConstants.AccessDeniedControllerName,
                                action = "Index",
                                hashedAccountId = hashedAccountId
                            }));
                }
            }
            catch (Exception)
            {
                filterContext.Result = new Microsoft.AspNetCore.Mvc.ViewResult { ViewName = ControllerConstants.BadRequestViewName };
            }
        }
    }
}