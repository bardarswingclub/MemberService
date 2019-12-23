using System.ComponentModel.DataAnnotations;

namespace MemberService.Data.ValueTypes
{
    public enum QuestionType
    {
        Unknown,

        [Display(Name = "Et obligatorisk svar")]
        Radio,

        [Display(Name = "Valgfritt svar")]
        CheckBox
    }
}