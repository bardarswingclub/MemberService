using System.ComponentModel;

namespace MemberService.Data
{
    public enum QuestionType
    {
        Unknown,

        [DisplayName("Et obligatorisk svar")]
        Radio,

        [DisplayName("Valgfritt svar")]
        CheckBox
    }
}