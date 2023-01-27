using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.EmployerFinance.Web.Controllers;

    public class AccessDeniedController : Controller
{
        public IActionResult Index()
        {
            return View();
        }
    }