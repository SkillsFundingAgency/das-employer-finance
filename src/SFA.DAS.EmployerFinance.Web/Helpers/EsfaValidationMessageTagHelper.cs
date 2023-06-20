using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.EmployerFinance.Web.Helpers
{
    [HtmlTargetElement("span", Attributes = ValidationForAttributeName)]
    public class EsfaValidationMessageTagHelper : TagHelper
    {
        private const string ValidationForAttributeName = "esfa-validation-for";

        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression Property { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        protected IHtmlGenerator Generator { get; }

        public EsfaValidationMessageTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("id", $"{Property.Name}-error");

            var tagBuilder = Generator.GenerateValidationMessage(
                ViewContext,
                Property.ModelExplorer,
                Property.Name,
                message: string.Empty,
                tag: null,
                htmlAttributes: null);

            output.Content.SetHtmlContent(tagBuilder.InnerHtml);
        }
    }
}