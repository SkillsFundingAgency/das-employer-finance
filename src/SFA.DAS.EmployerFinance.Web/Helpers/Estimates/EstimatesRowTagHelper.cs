using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.EmployerFinance.Web.Helpers.Estimates;

[HtmlTargetElement("estimates-row"), RestrictChildren("estimates-month-card")]
public class EstimatesRowTagHelper: TagHelper
{
    [HtmlAttributeName("text")]
    public string Text { get; set; }
    
    [HtmlAttributeName("provisional")]
    public bool Provisional { get; set; }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("estimates-row", HtmlEncoder.Default);

        output.Content.AppendHtml(RenderMonth());
        output.Content.AppendHtml(RenderDivider());
        output.Content.AppendHtml(await output.GetChildContentAsync());
    }

    private TagBuilder RenderMonth()
    {
        var container = new TagBuilder("div");
        container.AddCssClass("month");

        var text = new TagBuilder("div");
        text.InnerHtml.Append(Text);
        container.InnerHtml.AppendHtml(text);

        if (Provisional)
        {
            container.InnerHtml.AppendHtml(RenderProvisionalTag());
        }

        return container;
    }

    private static TagBuilder RenderProvisionalTag()
    {
        var provisionalTag = new TagBuilder("div");
        provisionalTag.AddCssClass("provisional");
        provisionalTag.InnerHtml.Append("In progress");
        return provisionalTag;
    }

    private static TagBuilder RenderDivider()
    {
        var container = new TagBuilder("div");
        container.AddCssClass("divider");
        
        var circle = new TagBuilder("div");
        circle.AddCssClass("circle");
        container.InnerHtml.AppendHtml(circle);

        var lineContainer = new TagBuilder("div");
        lineContainer.AddCssClass("line");
        lineContainer.InnerHtml.AppendHtml(new TagBuilder("div"));
        container.InnerHtml.AppendHtml(lineContainer);
        
        return container;
    }
}