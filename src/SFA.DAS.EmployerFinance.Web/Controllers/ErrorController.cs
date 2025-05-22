namespace SFA.DAS.EmployerFinance.Web.Controllers;

[AllowAnonymous]
[Route("[Controller]")]
public class ErrorController : Controller
{
    [Route("{statuscode?}")]
    public IActionResult Error(int? statusCode)
    {
        ViewBag.HideNav = true;

        return statusCode switch
        {
            400 or 403 or 404 => View(statusCode.ToString()),
            _ => View(),
        };
    }
}