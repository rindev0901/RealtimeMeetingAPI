using Microsoft.AspNetCore.Identity;

namespace RealtimeMeetingAPI.Entities
{
    public class AppRole : IdentityRole<Guid>
    {
        public string? Description { get; set; } = string.Empty;
        public virtual ICollection<AppRoleClaim> RoleClaims { get; set; }

        public virtual ICollection<AppUserRole> UserRoles { get; set; }
    }
}
