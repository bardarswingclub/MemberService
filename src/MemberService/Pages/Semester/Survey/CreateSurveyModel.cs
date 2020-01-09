using System;

namespace MemberService.Pages.Semester.Survey
{
    public class CreateSurveyModel
    {
        public Guid SemesterId { get; set; }

        public string SemesterTitle { get; set; }
    }
}