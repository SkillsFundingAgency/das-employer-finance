using Microsoft.AspNetCore.Html;
using SFA.DAS.EmployerFinance.Queries.GetContent;

namespace SFA.DAS.EmployerFinance.Web.Helpers;

public interface IHtmlHelpers
{
    Task<HtmlString> GetContentByTypeAsync(string type);
}

public class HtmlHelpers(IMediator mediator) : IHtmlHelpers
{
    public async Task<HtmlString> GetContentByTypeAsync(string type)
    {
        var userResponse = await mediator.Send(new GetContentQuery
        {
            ContentType = type
        });

        return new HtmlString(userResponse.Content);
    }
} 