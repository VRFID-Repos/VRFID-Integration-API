using App.Common.Messages;
using System.ComponentModel.DataAnnotations;

namespace App.Entity.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = ValidationMessages.ValidationMessage_EmailRequired)]
        [EmailAddress(ErrorMessage = ValidationMessages.ValidationMessage_EmailInValid)]
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}
