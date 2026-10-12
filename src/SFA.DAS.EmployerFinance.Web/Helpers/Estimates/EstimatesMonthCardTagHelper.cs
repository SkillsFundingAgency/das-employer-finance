using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.EmployerFinance.Web.Helpers.Estimates;

[HtmlTargetElement("estimates-month-card"), RestrictChildren("line-item")]
public class EstimatesMonthCardTagHelper : TagHelper
{
    [HtmlAttributeName("balance")]
    public string Balance { get; set; }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("estimates-card", HtmlEncoder.Default);
        output.Content.AppendHtml(RenderHeader());
        output.Content.AppendHtml(RenderBody(await output.GetChildContentAsync()));
    }

    private static TagBuilder RenderBody(TagHelperContent content)
    {
        var body = new TagBuilder("div");
        body.AddCssClass("card-body");
        body.InnerHtml.AppendHtml(content);
        return body;
    }

    private TagBuilder RenderHeader()
    {
        var header = new TagBuilder("div");
        header.AddCssClass("card-header");

        var text = new TagBuilder("div");
        text.InnerHtml.Append("Levy balance end of month");
        header.InnerHtml.AppendHtml(text);
        
        var value = new TagBuilder("div");
        value.InnerHtml.Append(Balance);
        header.InnerHtml.AppendHtml(value);

        return header;
    }
}