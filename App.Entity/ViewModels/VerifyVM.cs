using App.Common.Messages;
using App.Entity.Validation;
using System.ComponentModel.DataAnnotations;

namespace App.Entity.ViewModels
{
    public class VerifyVM
    {
        [Required]
        public string AuthToken { get; set; } = string.Empty;

        [PasswordValidate]
        [Required(ErrorMessage = ValidationMessages.ValidationMessage_PasswordRequired)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.ValidationMessage_ConfirmPassword)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
