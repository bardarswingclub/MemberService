using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NodaTime;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MemberService.Services
{
    public static class Extensions
    {
        public static string FormatMoney(this decimal amount)
            => string.Format("kr {0:0},-", amount);

        public static string ToOsloDate(this DateTime utc)
            => utc.ToOsloZone().Date.ToString();

        public static string ToOsloDateTime(this DateTime utc)
            => utc.ToOsloZone().ToString();

        public static string ToOsloTime(this DateTime utc)
            => utc.ToOsloZone().TimeOfDay.ToString("HH:mm", CultureInfo.InvariantCulture);

        public static ZonedDateTime ToOsloZone(this DateTime utc)
            => Instant.FromDateTimeUtc(utc.WithKind(DateTimeKind.Utc)).InZone(Constants.TimeZoneOslo);

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> predicate)
            => Expression.Lambda<Func<T, bool>>(Expression.Not(predicate.Body), predicate.Parameters);

        private static DateTime WithKind(this DateTime dateTime, DateTimeKind kind)
            => DateTime.SpecifyKind(dateTime, kind);

        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
            => enumValue.GetType()
                .GetMember(enumValue.ToString())
                .Single()
                .GetCustomAttribute<TAttribute>();

        public static string DisplayName(this Enum enumValue)
            => enumValue.GetAttribute<DisplayNameAttribute>().DisplayName;

        public static Guid? ToGuid(this string value)
            => Guid.TryParse(value, out var result) ? result : default;

        public static IHtmlContent Markdown<T>(this IHtmlHelper<T> html, string value)
            => html.Raw(Markdig.Markdown.ToHtml(value ?? string.Empty));
        public static string Slugify(this string phrase, int max = 45)
        {
            string str = phrase.RemoveAccent().ToLower();
            // invalid chars
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim
            str = str.Substring(0, str.Length <= max ? str.Length : max).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens
            return str;
        }

        public static string RemoveAccent(this string txt)
        {
            var bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static string ActionLink(this IUrlHelper url, string action, string controller, object values)
            => url.Action(
            action,
            controller,
            values,
            url.ActionContext.HttpContext.Request.Scheme,
            url.ActionContext.HttpContext.Request.Host.Value);

        public static string PageLink(this IUrlHelper url, string pageName, object values)
            => url.Page(
            pageName,
            null,
            values,
            url.ActionContext.HttpContext.Request.Scheme,
            url.ActionContext.HttpContext.Request.Host.Value);
    }
}
