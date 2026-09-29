using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.EmployerFinance.Web.Helpers;

[HtmlTargetElement("estimates-month-card"), RestrictChildren("line-item")]
public class EstimatesTagHelper : TagHelper
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

    private TagBuilder RenderBody(TagHelperContent content)
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
        
        // render the enclosing tag
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

[HtmlTargetElement("sub-item", ParentTag = "line-item")]
public class EstimatesSubItemTagHelper : TagHelper
{
    [HtmlAttributeName("text")]
    public string Text { get; set; }
    
    [HtmlAttributeName("value")]
    public string Value { get; set; }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
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