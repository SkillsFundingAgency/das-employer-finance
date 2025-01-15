using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Authorize(Policy = ApiRoles.ReadUserAccounts)]
[Route("api/user")]
public class UserController(IMediator mediator, ILogger<UserController> logger) : ControllerBase
{
    [HttpPut]
    [Route("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertRegisteredUserCommand command)
    {
        try
        {
            await mediator.Send(command);
            return Ok();
        }
        catch(Exception e)
        {
            logger.LogError(e,"Error in UserController PUT");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}