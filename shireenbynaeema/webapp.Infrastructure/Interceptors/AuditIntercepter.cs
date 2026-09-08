namespace Infrastructure
{
using System.Text.Json;
using System.Text.Json.Serialization;

using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedServices;

/// <summary>
/// Provides an Entity Framework Core interceptor that applies auditing and soft delete logic during save operations.
/// Audit logs are generated for changes to tracked entities, and hard deletes are converted to soft deletes for
/// entities supporting soft deletion.
/// </summary>
/// <remarks>AuditInterceptor enables automatic tracking of entity changes and user actions within a DbContext. It
/// ensures that audit information is recorded and that entities implementing soft deletion are not physically removed
/// from the database. The interceptor integrates with Entity Framework Core's interception pipeline and is suitable for
/// use in applications requiring compliance or traceability of data modifications.</remarks>
/// <param name="auditLogService">The audit log service used to persist audit log entries generated during save operations.</param>
/// <param name="auditScope">The audit scope that controls auditing behavior and holds the current audit logs for the operation.</param>
public class AuditInterceptor(IAuditLogService auditLogService, IAuditScope auditScope) : SaveChangesInterceptor
{
    private const string SoftDeletedAction = "SoftDeleted";

    /// <summary>
    /// Provides default JSON serialization options used for serializing and deserializing JSON data.
    /// </summary>
    /// <remarks>The options specify that output is not indented and null values are ignored during
    /// serialization. These settings ensure consistent JSON formatting and omission of null properties in serialized
    /// objects.</remarks>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Called before changes are saved to the database to allow interception or modification of the save operation.
    /// </summary>
    /// <remarks>
    /// Override this method to implement custom logic before changes are persisted, such as
    /// auditing, soft deletes, or modifying entity state. This method is called synchronously before SaveChanges or
    /// SaveChangesAsync completes.
    /// </remarks>
    /// <param name="eventData">
    /// Contextual information about the save operation, including the associated DbContext instance. Cannot be null.
    /// </param>
    /// <param name="result">
    /// The current interception result for the save operation, which can be used to suppress or modify the operation.
    /// </param>
    /// <returns>
    /// An <see cref="InterceptionResult{T}"/> where T is <see cref="int"/>.
    /// </returns>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null)
        {
            return base.SavingChanges(eventData, result);
        }

        var userId = CurrentUser.UserId ?? Constants.Seed.AdminUserId;
        var username = CurrentUser.Username;
        var ipAddress = CurrentUser.IPAddress;

        // Handle soft deletes first (before processing audit)
        this.HandleSoftDeletes(context, userId);

        if (auditScope.SkipAudit)
        {
            this.SetAuditableTimestamps(context, userId);
            return base.SavingChanges(eventData, result);
        }

        foreach (var entry in context.ChangeTracker.Entries<Auditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn ??= DateTimeExtension.UtcNowUnixTimestamp;
                    entry.Entity.CreatedBy ??= userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedOn = DateTimeExtension.UtcNowUnixTimestamp;
                    entry.Entity.UpdatedBy = userId;
                    break;
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                case EntityState.Deleted:
                    break;
                default:
                    break;
            }
        }

        var auditLogs = this.BuildAuditLogs(context, userId, username, ipAddress);

        if (auditLogs.Count > 0)
        {
            auditLogService.SaveLogs(auditLogs);
        }

        auditScope.CurrentAuditLogs = auditLogs;

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts the asynchronous save operation to handle soft deletes, update audit timestamps, and record audit
    /// logs before changes are committed to the database.
    /// </summary>
    /// <remarks>This method updates audit information and handles soft deletes for entities tracked by the
    /// DbContext. If auditing is enabled, audit logs are generated and persisted before the save operation completes.
    /// The method is intended to be used as part of an Entity Framework Core interception pipeline and is thread-safe
    /// for concurrent operations.</remarks>
    /// <param name="eventData">The event data associated with the save operation, including the current DbContext instance. Cannot be null.</param>
    /// <param name="result">The current interception result for the save operation, which can be used to suppress or modify the outcome.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous operation.</param>
    /// <returns>A ValueTask containing the interception result for the save operation. The result may be modified based on
    /// auditing or soft delete processing.</returns>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var userId = CurrentUser.UserId ?? Constants.Seed.AdminUserId;
        var username = CurrentUser.Username;
        var ipAddress = CurrentUser.IPAddress;

        // Handle soft deletes first (before processing audit)
        this.HandleSoftDeletes(context, userId);

        if (auditScope.SkipAudit)
        {
            this.SetAuditableTimestamps(context, userId);
            return await this.SavingChangesAsync(eventData, result, cancellationToken);
        }

        this.SetAuditableTimestamps(context, userId);

        var auditLogs = this.BuildAuditLogs(context, userId, username, ipAddress);

        if (auditLogs.Count > 0)
        {
            await auditLogService.SaveLogsAsync(auditLogs);
        }

        auditScope.CurrentAuditLogs = auditLogs;

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Converts hard deletes to soft deletes for entities implementing ISoftDeletable.
    /// </summary>
    private void HandleSoftDeletes(DbContext context, Guid userId)
    {
        var softDeletableEntries = context.ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in softDeletableEntries)
        {
            // Change from Deleted to Modified
            entry.State = EntityState.Modified;

            // Set soft delete properties
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedOn = DateTimeExtension.UtcNowUnixTimestamp;
            entry.Entity.DeletedBy = userId;

            // Mark only soft delete properties as modified to avoid triggering other validations
            entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified = true;
            entry.Property(nameof(ISoftDeletable.DeletedOn)).IsModified = true;
            entry.Property(nameof(ISoftDeletable.DeletedBy)).IsModified = true;
        }
    }

    /// <summary>
    /// Sets CreatedOn/CreatedBy for new entities and UpdatedOn/UpdatedBy for modified entities.
    /// </summary>
    private void SetAuditableTimestamps(DbContext context, Guid userId)
    {
        foreach (var entry in context.ChangeTracker.Entries<Auditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn ??= DateTimeExtension.UtcNowUnixTimestamp;
                    entry.Entity.CreatedBy ??= userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedOn = DateTimeExtension.UtcNowUnixTimestamp;
                    entry.Entity.UpdatedBy = userId;
                    break;
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                case EntityState.Deleted:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Builds audit log entries for all changed Auditable entities.
    /// </summary>
    private List<AuditLog> BuildAuditLogs(DbContext context, Guid userId, string? username, string? ipAddress)
    {
        var auditLogs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not Auditable)
            {
                continue;
            }

            if (entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            var action = this.GetAuditAction(entry);
            var changes = this.GetChanges(entry);

            if (changes.Count == 0)
            {
                continue;
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTimeExtension.UtcNowUnixTimestamp,
                UserId = userId.ToString(),
                Username = username ?? Constants.Seed.AdminDisplayName,
                IPAddress = ipAddress,
                EntityName = entry.Entity.GetType().Name,
                Action = action,
                EntityId = entry.Properties
                    .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString(),
                Changes = JsonSerializer.Serialize(changes, JsonOptions),
            };

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }

    /// <summary>
    /// Determines the audit action, detecting soft deletes.
    /// </summary>
    private string GetAuditAction(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        // Check if this is a soft delete (Modified state with IsDeleted = true)
        if (entry.State == EntityState.Modified && entry.Entity is ISoftDeletable softDeletable)
        {
            var isDeletedProperty = entry.Property(nameof(ISoftDeletable.IsDeleted));

            // If IsDeleted changed from false to true, it's a soft delete
            if (isDeletedProperty.IsModified &&
                isDeletedProperty.OriginalValue is false &&
                isDeletedProperty.CurrentValue is true)
            {
                return SoftDeletedAction;
            }
        }

        return entry.State.ToString();
    }

    /// <summary>
    /// Extracts property changes for the audit log.
    /// </summary>
    private List<AuditEntry> GetChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var changes = new List<AuditEntry>();

        foreach (var prop in entry.Properties)
        {
            if (prop.IsTemporary)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    changes.Add(new AuditEntry
                    {
                        PropertyName = prop.Metadata.Name,
                        OldValue = null,
                        NewValue = prop.CurrentValue,
                    });
                    break;

                case EntityState.Deleted:
                    changes.Add(new AuditEntry
                    {
                        PropertyName = prop.Metadata.Name,
                        OldValue = prop.OriginalValue,
                        NewValue = null,
                    });
                    break;

                case EntityState.Modified:
                    if (!Equals(prop.OriginalValue, prop.CurrentValue))
                    {
                        changes.Add(new AuditEntry
                        {
                            PropertyName = prop.Metadata.Name,
                            OldValue = prop.OriginalValue,
                            NewValue = prop.CurrentValue,
                        });
                    }

                    break;
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                default:
                    break;
            }
        }

        return changes;
    }
}
}