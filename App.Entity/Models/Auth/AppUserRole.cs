using Microsoft.AspNetCore.Identity;

namespace App.Entity.Models.Auth
{
    public class AppUserRole : IdentityRole
    {
        public virtual ICollection<AppUserRoleMapping>? RoleMappings { get; set; }
    }
}
