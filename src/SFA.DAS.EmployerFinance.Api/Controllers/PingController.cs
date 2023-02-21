namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}