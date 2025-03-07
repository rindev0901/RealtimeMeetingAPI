using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RealtimeMeetingAPI.Interfaces;

namespace RealtimeMeetingAPI.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null)
            {
                return base.SavedChangesAsync(eventData, result, cancellationToken);
            }

            var entries =
                eventData
                    .Context
                    .ChangeTracker
                    .Entries<ISoftDeletable>()
                    .Where(e => e.State == EntityState.Deleted);

            foreach (var entityEntry in entries)
            {
                entityEntry.State = EntityState.Modified;
                entityEntry.Entity.IsDeleted = true;
                entityEntry.Entity.DeletedOnUtc = DateTime.UtcNow;
            }

            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
