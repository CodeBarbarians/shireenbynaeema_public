namespace Domain
{
/// <summary>
/// Marks an entity as supporting soft deletion without physically removing the record from storage.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>Gets or sets a value indicating whether indicates whether the entity has been soft deleted.</summary>
    bool IsDeleted { get; set; }

    /// <summary>Gets or sets unix timestamp when the entity was deleted.</summary>
    long? DeletedOn { get; set; }

    /// <summary>Gets or sets identifier of the user who performed the deletion.</summary>
    Guid? DeletedBy { get; set; }
}
}