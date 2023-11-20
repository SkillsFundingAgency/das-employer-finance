namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [Route("[Controller]")]
    [Route("error/403")]
    public class AccessDeniedController : Controller
    {
        public IActionResult Index()
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return View();
        }
    }
}
