namespace MemberService.Data.ValueTypes;

using System.ComponentModel.DataAnnotations;

public enum QuestionType
{
    Unknown,

    [Display(Name = "Et obligatorisk svar")]
    Radio,

    [Display(Name = "Valgfritt svar")]
    CheckBox
}
