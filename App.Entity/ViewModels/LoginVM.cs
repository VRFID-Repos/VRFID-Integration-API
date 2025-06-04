using App.Common.Messages;
using System.ComponentModel.DataAnnotations;
namespace App.Entity.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = ValidationMessages.ValidationMessage_EmailRequired)]
        [EmailAddress(ErrorMessage = ValidationMessages.ValidationMessage_EmailInValid)]
        public string? Email { get; set; }

        [Required(ErrorMessage = ValidationMessages.ValidationMessage_PasswordRequired)]
        public string? Password { get; set; }
        public bool KeepAlive { get; set; }
    }
}
