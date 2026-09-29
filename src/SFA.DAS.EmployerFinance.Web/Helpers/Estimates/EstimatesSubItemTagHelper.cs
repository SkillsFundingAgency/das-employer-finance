using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.EmployerFinance.Web.Helpers.Estimates;

[HtmlTargetElement("sub-item", ParentTag = "line-item")]
public class EstimatesSubItemTagHelper : TagHelper
{
    [HtmlAttributeName("text")]
    public string Text { get; set; }
    
    [HtmlAttributeName("value")]
    public string Value { get; set; }
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("sub-item", HtmlEncoder.Default);
        
        var text = new TagBuilder("div");
        text.InnerHtml.Append(Text);
        output.Content.AppendHtml(text);
        
        var value = new TagBuilder("div");
        value.InnerHtml.Append(Value);
        output.Content.AppendHtml(value);
    }
}