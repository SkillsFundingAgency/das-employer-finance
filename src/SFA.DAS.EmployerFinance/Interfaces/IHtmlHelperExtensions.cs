using Microsoft.AspNetCore.Html;

namespace SFA.DAS.EmployerFinance.Interfaces
{
    public interface IHtmlHelperExtensions
    {
        HtmlString CdnLink(string folderName, string fileName);

    }
}
