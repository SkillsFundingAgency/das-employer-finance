using System;
using System.Net;
using SFA.DAS.Authentication;
using SFA.DAS.EmployerFinance.Interfaces;
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

       public BaseController() { }

        public void AddFlashMessageToCookie(FlashMessageViewModel model)
        {
            _flashMessage.Delete(FlashMessageCookieName);

            _flashMessage.Create(model, FlashMessageCookieName);
        }

        public FlashMessageViewModel GetFlashMessageViewModelFromCookie()
        {
            var flashMessageViewModelFromCookie = _flashMessage.Get(FlashMessageCookieName);
            _flashMessage.Delete(FlashMessageCookieName);
            return flashMessageViewModelFromCookie;
        }

    }
}