using Microsoft.AspNetCore.Identity;

namespace RealtimeMeetingAPI.Entities
{
    public class AppRoleClaim : IdentityRoleClaim<Guid>
    {
        public virtual AppRole Role { get; set; }
    }
}
