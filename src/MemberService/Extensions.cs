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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Event;

namespace MemberService
{
    public static class Extensions
    {
        public static string FormatMoney(this decimal amount)
            => $"kr {amount:0},-";

        public static string ToOsloDate(this DateTime utc)
            => utc.ToOsloZone().Date.ToString();

        public static string ToOsloDateTime(this DateTime utc)
            => utc.ToOsloZone().ToString();

        public static string ToOsloTime(this DateTime utc)
            => utc.ToOsloZone().TimeOfDay.ToString("HH:mm", CultureInfo.InvariantCulture);

        public static ZonedDateTime ToOsloZone(this DateTime utc)
            => Instant.FromDateTimeUtc(utc.WithKind(DateTimeKind.Utc)).InZone(TimeProvider.TimeZoneOslo);

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> predicate)
            => Expression.Lambda<Func<T, bool>>(Expression.Not(predicate.Body), predicate.Parameters);

        private static DateTime WithKind(this DateTime dateTime, DateTimeKind kind)
            => DateTime.SpecifyKind(dateTime, kind);

        public static string DisplayName(this Enum enumValue)
            => enumValue.GetAttribute<DisplayAttribute>().Name;

        public static string DisplayDescription(this Enum enumValue)
            => enumValue.GetAttribute<DisplayAttribute>()?.Description ?? enumValue.GetAttribute<DescriptionAttribute>().Description;

        private static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
            => enumValue.GetType()
                .GetMember(enumValue.ToString())
                .Single()
                .GetCustomAttribute<TAttribute>();

        public static Guid? ToGuid(this string value)
            => Guid.TryParse(value, out var result) ? result : default;

        public static IHtmlContent Markdown<T>(this IHtmlHelper<T> html, string value)
            => html.Raw(Markdig.Markdown.ToHtml(value ?? String.Empty));

        public static string Slugify(this string phrase)
            => phrase.GenerateSlug();

        public static string ActionLink(this IUrlHelper url, string action, string controller, object values)
            => url.Action(
            action,
            controller,
            values,
            url.ActionContext.HttpContext.Request.Scheme,
            url.ActionContext.HttpContext.Request.Host.Value);

        public static string RolesCount(this EventEntry model, Status? status = null)
        {
            var statuses = model.Signups
                .Where(s => status != null
                    ? s.Status == status
                    : s.Status != Status.RejectedOrNotPayed && s.Status != Status.Denied)
                .ToReadOnlyCollection();

            if (model.RoleSignup)
            {
                var leads = statuses.Count(s => s.Role == DanceRole.Lead);
                var follows = statuses.Count(s => s.Role == DanceRole.Follow);
                return $"{leads}+{follows}";
            }

            return statuses.Count(s => s.Role == DanceRole.None).ToString();
        }

        public static void Add(this ICollection<EventSignupAuditEntry> collection, string message, User user, DateTime? occuredAtUtc = null)
            => collection.Add(new EventSignupAuditEntry
            {
                User = user,
                Message = message,
                OccuredAtUtc = occuredAtUtc ?? TimeProvider.UtcNow
            });

        public static T GetOrAdd<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<T> factory)
        {
            var result = collection.FirstOrDefault(predicate);
            if (result == null)
            {
                result = factory();
                collection.Add(result);
            }

            return result;
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
            => source.Select((item, index) => (item, index));


        public static (string Date, string Time) GetLocalDateAndTime(this DateTime? utc)
        {
            return utc?.GetLocalDateAndTime() ?? (null, null);
        }

        public static (string Date, string Time) GetLocalDateAndTime(this DateTime utc)
        {
            var result = utc.ToOsloZone();

            var date = result.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var time = result.TimeOfDay.ToString("HH:mm", CultureInfo.InvariantCulture);

            return (date, time);
        }
    }
}
