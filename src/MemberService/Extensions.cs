using Clave.ExtensionMethods;
using MemberService.Data;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NodaTime;
using SlugGenerator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MemberService
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

        public static string Slugify(this string phrase)
            => phrase.GenerateSlug();

        public static string ActionLink(this IUrlHelper url, string action, string controller, object values)
            => url.Action(
            action,
            controller,
            values,
            url.ActionContext.HttpContext.Request.Scheme,
            url.ActionContext.HttpContext.Request.Host.Value);

        public static string RolesCount(this Event model, Status? status = null)
        {
            var statuses = model.Signups
                .Where(s => status != null
                    ? s.Status == status
                    : s.Status != Status.RejectedOrNotPayed && s.Status != Status.Denied)
                .ToReadOnlyCollection();

            if (model.SignupOptions.RoleSignup)
            {
                var leads = statuses.Count(s => s.Role == DanceRole.Lead);
                var follows = statuses.Count(s => s.Role == DanceRole.Follow);
                return $"{leads}+{follows}";
            }

            return statuses.Count(s => s.Role == DanceRole.None).ToString();
        }

        public static void Add(this ICollection<EventSignupAuditEntry> collection, string message, MemberUser user)
            => collection.Add(new EventSignupAuditEntry
            {
                User = user,
                Message = message,
                OccuredAtUtc = DateTime.UtcNow
            });
    }
}
