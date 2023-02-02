using System;
using System.Net;
using SFA.DAS.Authentication;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    public class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
        private const string FlashMessageCookieName = "sfa-das-employerapprenticeshipsservice-flashmessage";

        private readonly ICookieStorageService<FlashMessageViewModel> _flashMessage;

        public BaseController(ICookieStorageService<FlashMessageViewModel> flashMessage)
        {
            _flashMessage = flashMessage;
        }

        protected override Microsoft.AspNetCore.Mvc.ViewResult View(string viewName, string masterName, object model)
        {
            var orchestratorResponse = model as OrchestratorResponse;

            if (orchestratorResponse == null)
            {
                return base.View(viewName, masterName, model);
            }

            var invalidRequestException = orchestratorResponse.Exception as InvalidRequestException;

            if (invalidRequestException != null)
            {
                foreach (var errorMessageItem in invalidRequestException.ErrorMessages)
                {
                    ModelState.AddModelError(errorMessageItem.Key, errorMessageItem.Value);
                }

                if (orchestratorResponse.Status == HttpStatusCode.BadRequest)
                {
                    return ReturnViewResult(ControllerConstants.BadRequestViewName, masterName, orchestratorResponse);
                }

                return ReturnViewResult(viewName, masterName, orchestratorResponse);
            }

            if (orchestratorResponse.Status == HttpStatusCode.OK)
            {
                return ReturnViewResult(viewName, masterName, orchestratorResponse);
            }

            if (orchestratorResponse.Status >= HttpStatusCode.BadRequest)
            {
                throw new HttpException((int)orchestratorResponse.Status, orchestratorResponse.Status.ToString());
            }

            if (orchestratorResponse.Exception != null)
            {
                throw orchestratorResponse.Exception;
            }

            throw new Exception($"Orchestrator response of type '{model.GetType()}' could not be handled.");
        }
        private Microsoft.AspNetCore.Mvc.ViewResult ReturnViewResult(string viewName, string masterName, OrchestratorResponse orchestratorResponse)
        {
            return base.View(viewName, masterName, orchestratorResponse);
        }
    }
}