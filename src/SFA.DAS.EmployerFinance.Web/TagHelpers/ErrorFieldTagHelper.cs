using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.EmployerFinance.Web.TagHelpers;

[HtmlTargetElement("div", Attributes = "asp-fieldname")]
public class ErrorFieldTagHelper : TagHelper
{
    [ViewContext]
    [HtmlAttributeNotBound]
    // ReSharper disable once MemberCanBePrivate.Global - must be public for ViewContext to be set
    public ViewContext ViewContext { get; set; }

    [HtmlAttributeName("asp-fieldname")]
    public string FieldName { get; set; }
    
    [HtmlAttributeName("asp-additionalclass")]
    public string AdditionalClass { get; set; }


    
    public override void Process(TagHelperContext context, TagHelperOutput tagHelperOutput)
    {
        var fieldNames = FieldName.Split(",");
        if (ViewContext?.ModelState != null)
        {
            if (fieldNames.Any(modelStateKey => ViewContext.ModelState.ContainsKey(modelStateKey) &&
                                                ViewContext.ModelState[modelStateKey]!.Errors.Any()))
            {
                tagHelperOutput.AddClass("govuk-form-group--error", HtmlEncoder.Default);
            }
            
            if (string.IsNullOrEmpty(AdditionalClass))
            {
                tagHelperOutput.AddClass(AdditionalClass, HtmlEncoder.Default);
            }
        }
        tagHelperOutput.AddClass("govuk-form-group", HtmlEncoder.Default);
        
        tagHelperOutput.TagMode = TagMode.StartTagAndEndTag;
    }
}