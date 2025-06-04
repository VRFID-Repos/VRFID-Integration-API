using App.Entity.Models.Admin;
using App.Entity.Models.Auth;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Entity.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Designation { get; set; }

        public int? CountryId { get; set; }

        public string? AboutMe { get; set; }
        public string? Signature { get; set; }
        public string? Profile { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(50)]
        public string? EmployementStatus { get; set; }

        public virtual ICollection<AppUserRoleMapping>? UserRoleMappings { get; set; }

		public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DOB { get; set; }
        public string? DrivingLicence { get; set; }
        public string? Notes { get; set; }
        public int? IsApproved { get; set; }

    }
}
