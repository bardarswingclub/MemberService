namespace MemberService.Pages.Shared;


using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class EnforceTrueAttribute : ValidationAttribute, IClientModelValidator
{
    public override bool IsValid(object value)
    {
        if (value == null) return false;
        if (value.GetType() != typeof(bool)) throw new InvalidOperationException("can only be used on boolean properties.");
        return (bool)value == true;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-enforcetrue"] = ErrorMessage;

    }
}
