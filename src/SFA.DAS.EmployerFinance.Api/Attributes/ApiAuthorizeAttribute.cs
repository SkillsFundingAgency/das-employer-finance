namespace SFA.DAS.EmployerFinance.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiAuthorizeAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute
    {
        //protected override bool IsAuthorized(HttpActionContext actionContext)
        //{
        //    return actionContext.Request.RequestUri.IsLoopback || base.IsAuthorized(actionContext);
        //}
    }
}