using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.EmployerFinance.Web.Helpers.Estimates;

[HtmlTargetElement("line-item", ParentTag = "estimates-month-card"), RestrictChildren("sub-item")]
public class EstimatesLineItemTagHelper : TagHelper
{
    [HtmlAttributeName("text")]
    public string Text { get; set; }
    
    [HtmlAttributeName("value")]
    public string Value { get; set; }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();
        RenderContent(output, content);
    }
    
    private void RenderContent(TagHelperOutput output, TagHelperContent content)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        
        if (content.IsEmptyOrWhiteSpace)
        {
            RenderLineItem(output);
            return;
        }
        
        RenderLineItemWithSubItems(output, content);
    }

    private void RenderLineItemWithSubItems(TagHelperOutput output, TagHelperContent content)
    {
        output.AddClass("line-item-container", HtmlEncoder.Default);
        
        var lineItem = new TagBuilder("div");
        lineItem.AddCssClass("line-item");
        
        lineItem.InnerHtml.AppendHtml(CreateTextTag());
        lineItem.InnerHtml.AppendHtml(CreateValueTag());
        
        output.Content.AppendHtml(lineItem);
        output.Content.AppendHtml(content);
    }

    private void RenderLineItem(TagHelperOutput output)
    {
        output.AddClass("line-item", HtmlEncoder.Default);
        output.Content.AppendHtml(CreateTextTag());
        output.Content.AppendHtml(CreateValueTag());
    }
    
    private TagBuilder CreateTextTag()
    {
        var text = new TagBuilder("div");
        text.InnerHtml.Append(Text);
        return text;
    }
    
    private TagBuilder CreateValueTag()
    {
        var value = new TagBuilder("div");
        value.InnerHtml.Append(Value);
        return value;
    }
}