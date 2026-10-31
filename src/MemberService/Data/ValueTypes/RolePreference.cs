namespace MemberService.Data.ValueTypes;

using System.ComponentModel.DataAnnotations;

[Flags]
public enum RolePreference
{
    None = 0,

    [Display(Name = "Fører")]
    Lead = 1,

    [Display(Name = "Følger")]
    Follow = 2,

    [Display(Name = "Switch")]
    Switch = 4
}

/// <summary>The role choices shown in a signup form, and which one is selected.</summary>
public record RoleChoices(IReadOnlyCollection<RolePreference> Allowed, RolePreference Selected, bool Required = true)
{
    public static RoleChoices For(RolePreference allowed, RolePreference selected, bool required = true)
        => new(allowed.ToChoices(), selected, required);
}

public static class RolePreferenceExtensions
{
    private static readonly RolePreference[] Choices = { RolePreference.Lead, RolePreference.Follow, RolePreference.Switch };

    public const RolePreference Everything = RolePreference.Lead | RolePreference.Follow | RolePreference.Switch;

    public const RolePreference LeadAndFollow = RolePreference.Lead | RolePreference.Follow;

    /// <summary>The allowed roles of an event; an empty selection falls back to lead and follow.</summary>
    public static RolePreference OrDefault(this RolePreference allowed)
        => allowed == RolePreference.None ? LeadAndFollow : allowed;

    /// <summary>True for exactly one of lead, follow or switch.</summary>
    public static bool IsSingleChoice(this RolePreference preference)
        => preference != RolePreference.None && Enum.IsDefined(preference);

    public static RolePreference Combine(this IEnumerable<RolePreference> preferences)
        => preferences?.Aggregate(RolePreference.None, (all, p) => all | p) ?? RolePreference.None;

    /// <summary>The role used for grouping lists: only set when exactly one of lead/follow is chosen.</summary>
    public static DanceRole ToRole(this RolePreference preference)
        => preference switch
        {
            RolePreference.Lead => DanceRole.Lead,
            RolePreference.Follow => DanceRole.Follow,
            _ => DanceRole.None
        };

    public static RolePreference FromRole(DanceRole role)
        => role switch
        {
            DanceRole.Lead => RolePreference.Lead,
            DanceRole.Follow => RolePreference.Follow,
            _ => RolePreference.None
        };

    public static IReadOnlyCollection<RolePreference> ToChoices(this RolePreference preference)
        => Choices.Where(c => preference.HasFlag(c)).ToList();

    public static string Summary(this RolePreference preference)
        => string.Join(", ", Choices.Where(c => preference.HasFlag(c)).Select(c => c.DisplayName()));
}
