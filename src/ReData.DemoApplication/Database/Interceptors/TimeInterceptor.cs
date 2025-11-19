using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Database.Entities.Abstract;

namespace ReData.DemoApplication.Database.Interceptors;

public sealed class TimeInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private static void SetTimestamps(DbContext context)
    {
        if (context is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<ICreatedAt>())
        {
            if (entry.State is EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<IUpdatedAt>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}