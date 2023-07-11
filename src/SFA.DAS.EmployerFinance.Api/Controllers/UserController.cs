using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Authorize(Policy = ApiRoles.ReadUserAccounts)]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    public UserController(IMediator mediator, ILogger<UserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpPut]
    [Route("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertRegisteredUserCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch(Exception e)
        {
            _logger.LogError(e,"Error in UserController PUT");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}