namespace MemberService.Pages.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("switch")]
public class SwitchTagHelper : TagHelper
{
    public static readonly object Key = new();

    [HtmlAttributeName("value")]
    public object Value { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        context.Items[Key] = Value;

        var childContent = await output.GetChildContentAsync();

        output.TagName = null;
        output.PostContent.SetHtmlContent(childContent);
    }
}

[HtmlTargetElement("case")]
public class CaseTagHelper : TagHelper
{
    [HtmlAttributeName("value")]
    public object Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var value = context.Items[SwitchTagHelper.Key];
        if (!context.AllAttributes.Any(a => Equals(a.Value, value)))
            output.SuppressOutput();
    }
}
