namespace MemberService.Pages.Event;



using System.ComponentModel.DataAnnotations;

using MemberService.Data.ValueTypes;

public class EventSaveModel
{
    [Required]
    public Status Status { get; set; }

    public List<Item> Leads { get; set; } = new();

    public List<Item> Follows { get; set; } = new();

    public List<Item> Solos { get; set; } = new();

    public bool SendEmail { get; set; }

    public bool ReplyToMe { get; set; }

    [Required]
    public string Subject { get; set; }

    [Required]
    public string Message { get; set; }

    public class Item
    {
        public Guid Id { get; set; }

        public bool Selected { get; set; }
    }
}
