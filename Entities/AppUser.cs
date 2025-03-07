using Microsoft.AspNetCore.Identity;
using RealtimeMeetingAPI.Interfaces;

namespace RealtimeMeetingAPI.Entities
{
    public class AppUser : IdentityUser<Guid>, IBaseEntity, ISoftDeletable
    {
        public string FullName { get; set; } = string.Empty;
        public bool Locked { get; set; } = false;
        public string PhotoUrl { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActive { get; set; } = null;
        public DateTime? ModifiedAt { get; set; } = null;
        public DateTime? DeletedOnUtc { get; set; } = null;

        public virtual ICollection<AppUserRole> UserRoles { get; set; }
        public virtual ICollection<IdentityUserClaim<Guid>> Claims { get; set; }
        public virtual ICollection<IdentityUserLogin<Guid>> Logins { get; set; }
        public virtual ICollection<IdentityUserToken<Guid>> Tokens { get; set; }
    }
}
