using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[AllowAnonymous]
[Route("")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}