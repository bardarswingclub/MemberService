namespace MemberService.Tests;

using System.Reflection;

using MemberService.Pages.Event;
using MemberService.Pages.Event.Presence;
using MemberService.Pages.Pay;

using Microsoft.AspNetCore.Mvc;

using NUnit.Framework;

using Shouldly;

public class AntiforgeryTests
{
    [Test]
    public void ChangingActions_ValidateAntiforgeryToken()
    {
        var actions = new[] { typeof(EventController), typeof(PresenceController) }
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Append(typeof(PayController).GetMethod(nameof(PayController.Fee)))
            .Where(m => m.GetCustomAttributes().Any(a => a is HttpPostAttribute or HttpPutAttribute or HttpDeleteAttribute))
            .ToList();

        actions.ShouldNotBeEmpty();
        actions
            .Where(m => m.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() is null)
            .Select(m => $"{m.DeclaringType.Name}.{m.Name}")
            .ShouldBeEmpty();
    }
}
