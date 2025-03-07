using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealtimeMeetingAPI.Entities;

namespace RealtimeMeetingAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<
        AppUser, AppRole, Guid,
        IdentityUserClaim<Guid>, AppUserRole, IdentityUserLogin<Guid>,
        AppRoleClaim, IdentityUserToken<Guid>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Config soft deleted
            builder.Entity<AppUser>().HasQueryFilter(r => !r.IsDeleted);

            // Config relationship

            builder.Entity<AppUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne()
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            builder.Entity<AppRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });

            builder.Entity<AppUserRole>(b =>
            {
                // Define the composite primary key
                b.HasKey(ur => new { ur.UserId, ur.RoleId });

                // Define relationships
                b.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles) // Ensure AppUser has a collection of UserRoles
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles) // Ensure AppRole has a collection of UserRoles
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<IdentityUserClaim<Guid>>(b =>
            {
                b.ToTable("AppUserClaims");
            });

            builder.Entity<IdentityUserLogin<Guid>>(b =>
            {
                b.ToTable("AppUserLogins");
            });

            builder.Entity<IdentityUserToken<Guid>>(b =>
            {
                b.ToTable("AppUserTokens");
            });
            builder.Entity<AppUser>(b =>
            {
                b.ToTable("AppUser");
            });

            builder.Entity<AppRole>(b =>
            {
                b.ToTable("AppRole");
            });

            builder.Entity<AppUserRole>(b =>
            {
                b.ToTable("AppUserRole");
            });

            builder.Entity<AppRoleClaim>(b =>
            {
                b.ToTable("AppRoleClaim");
            });
        }
    }
}
