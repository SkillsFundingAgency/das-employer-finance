using Microsoft.AspNetCore.Html;
using SFA.DAS.EmployerFinance.Queries.GetContent;

namespace SFA.DAS.EmployerFinance.Web.Helpers;

public interface IHtmlHelpers
{
    HtmlString GetContentByType(string type);
}

public class HtmlHelpers(IMediator mediator) : IHtmlHelpers
{

    public HtmlString GetContentByType(string type)
    {
        var userResponse = mediator.Send(new GetContentQuery
        {
            ContentType = type
        }).GetAwaiter().GetResult();

        return new HtmlString(userResponse.Content);
    }
} 