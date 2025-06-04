using App.Common.Messages;
using App.Common.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Validation
{
    public class PasswordValidateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string password = value as string ?? "";
            if (PasswordUtil.ValidatePassword(password))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ValidationMessages.PasswordValidationError);
        }
    }
}
