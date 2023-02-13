using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.EmployerFinance.Web.Authentication;

public class EmployerAccountOwnerAuthorizationHandler: AuthorizationHandler<EmployerAccountRequirement>
{
    private readonly IEmployerAccountAuthorisationHandler _handler;

    public EmployerAccountOwnerAuthorizationHandler(IEmployerAccountAuthorisationHandler handler)
    {
        _handler = handler;
    }
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountRequirement requirement)
    {
        if (!(await _handler.IsEmployerAuthorised(context, false)))
        {
            return;
        }

        context.Succeed(requirement);

        return;
    }
}