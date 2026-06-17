using System.Reflection;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("info")]
public class InfoController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        return Content($"Employer Finance API - Version {version}", "text/plain");
    }
}
