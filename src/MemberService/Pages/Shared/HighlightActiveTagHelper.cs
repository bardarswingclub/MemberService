using System;
using System.Collections.Generic;
using System.Linq;
using Clave.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MemberService.Pages.Shared
{
    [HtmlTargetElement("a", Attributes = "highlight-active")]
    public class HighlightActiveTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private IUrlHelperFactory _urlHelper { get; set; }

        public HighlightActiveTagHelper(IUrlHelperFactory urlHelper)
        {
            _urlHelper = urlHelper;
        }

        [HtmlAttributeName("highlight-active-when")]
        public string Match { get; set; } = null;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Remove(output.Attributes["highlight-active"]);

            if (IsMatching(context, output))
            {
                var linkTag = new TagBuilder("a");
                linkTag.Attributes.Add("class", "active");
                output.MergeAttributes(linkTag);
            }
        }

        private bool IsMatching(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Match))
            {
                var urlHelper = _urlHelper.GetUrlHelper(ViewContext);

                var url = output.Attributes["href"].Value.ToString();

                return urlHelper.Action() == url;
            }
            else
            {
                var keys = Match.Split(' ');

                var currentRoute = ViewContext.HttpContext.Request.RouteValues;
                var targetRoute = context.AllAttributes;

                return keys
                    .Select(key => currentRoute.GetValueOrDefault(key)?.Equals(targetRoute[KeyToAttribute(key)]?.Value))
                    .All(x => x == true);
            }
        }

        private string KeyToAttribute(string key)
        {
            switch (key)
            {
                case "controller":
                case "action":
                case "page":
                    return $"asp-{key}";
                default:
                    return $"asp-route-{key}";
            }
        }
    }
}